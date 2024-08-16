using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;

using CameraLoader.Game;
using CameraLoader.Utils;

namespace CameraLoader.UI;

public class PresetTab : PresetTabBase
{
    private readonly string _tabName;
    private readonly int _modeCount;
    private readonly PresetHandler _presetHandler;
    private readonly FontAwesomeIcon _presetIcon;
    private readonly Vector4 _presetButtonColor;
    private readonly Action _presetInfoDrawFunc;

    private string _creationName;

    public PresetTab(Type presetType)
    {
        this._tabName = presetType.Name.Remove(presetType.Name.Length - "preset".Length);
        if (presetType == typeof(CameraPreset))
        {
            this._presetHandler = new CameraPresetHandler();
            this._modeCount = 2;
            this._presetIcon = FontAwesomeIcon.CameraRetro;
            this._presetButtonColor = ImGuiUtils.Green;
            this._presetInfoDrawFunc = DrawCamersPresetInfo;
        }
        else if (presetType == typeof(LightingPreset))
        {
            this._presetHandler = new LightingPresetHandler();
            this._modeCount = 3;
            this._presetIcon = FontAwesomeIcon.Lightbulb;
            this._presetButtonColor = ImGuiUtils.Yellow;
            this._presetInfoDrawFunc = DrawLightingPresetInfo;
        }
    }

    public void Draw(float selectBoxHeight, float infoBoxHeight)
    {
        if (!ImGui.BeginTabItem(this._tabName)) { return; }

        ImGui.Spacing();

        var isInGPose = this.IsInGPose();

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
        ImGui.BeginChild($"Preset Menu##{this._tabName}", new Vector2(0f, selectBoxHeight + (ImGuiUtils.FrameRounding * 2f)), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetFrameHeightWithSpacing());
        ImGui.InputTextWithHint("##Search", "Search...", ref this.SearchQuery, 30);
        ImGui.SameLine();
        ImGui.PopItemWidth();

        // Preset Creation Popup
        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, this._presetButtonColor, default, "Create a new preset"))
        {
            for (int i = 1; i <= this._presetHandler.GetPresets().Count + 1; i++)
            {
                this._creationName = $"Preset #{i}";
                if (!Service.Config.LightingPresetNames.Contains(this._creationName)) { break; }
            }
            ImGui.OpenPopup("Preset Menu##Create");
        }
        ImGui.PushItemWidth(100f * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginPopup("Preset Menu##Create"))
        {
            DrawModeSelection(this._modeCount);

            ImGui.InputTextWithHint("##Preset Create Input", "Preset Name", ref this._creationName, 30);
            if (ImGui.Button("Create Preset") && this._creationName.Length > 0)
            {
                this._presetHandler.Create(this._creationName, this.SelectedMode);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
        ImGui.PopItemWidth();

        // Preset Listing
        for (int i = 0; i < this._presetHandler.GetPresets().Count; i++)
        {
            var preset = this._presetHandler.GetPresets()[i];
            if (!preset.Name.ToLower().Contains(this.SearchQuery.ToLower())) { continue; }

            var isCurrentSelected = this.PresetIndex == i;
            if (ImGui.Selectable($"{preset.Name}", isCurrentSelected))
            {
                this.PresetIndex = isCurrentSelected ? -1 : i;
                this.SelectedPreset = isCurrentSelected ? null : preset;

                this.RenameOpen = false;
                this.ErrorMessage = "";
            }
        }
        ImGui.EndChild();

        // Preset Information
        if (this.SelectedPreset is not null)
        {
            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
            ImGui.BeginChild("Preset Detail", new Vector2(0f, infoBoxHeight + (ImGuiUtils.FrameRounding * 2f)), true, ImGuiWindowFlags.NoScrollbar);
            ImGui.PopStyleVar(1);

            ImGui.TextWrapped(this.SelectedPreset.Name);
            this._presetInfoDrawFunc();

            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                this.SelectedPreset.Load();
                this.ErrorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Rename", ImGuiUtils.Orange))
            {
                this.RenameOpen = !this.RenameOpen;
                this.ErrorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Remove", ImGuiUtils.Red))
            {
                this._presetHandler.Delete(this.SelectedPreset);
                this.PresetIndex = -1;
                this.SelectedPreset = null;
            }

            if (this.RenameOpen)
                DrawRename();

            this.DrawErrorMessage();
            ImGui.EndChild();
        }

        if (!isInGPose) { ImGui.EndDisabled(); }
        ImGui.EndTabItem();
    }

    private void DrawModeSelection(int count)
    {
        ImGuiUtils.IconText(this._presetIcon);
        ImGui.SameLine();

        ImGui.Text("Preset Mode:");
        ImGui.BeginGroup();
        foreach (var mode in Enum.GetValues<PresetMode>())
        {
            if ((int)mode > count - 1)
                continue;

            ImGui.RadioButton(mode.GetDescription(), ref this.SelectedMode, (int)mode);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(mode.GetTooltip());
        }
        ImGui.EndGroup();
    }

    private void DrawCamersPresetInfo()
    {
        var preset = (CameraPreset)this.SelectedPreset;
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));

        ImGuiUtils.IconText(FontAwesomeIcon.Paste);
        ImGui.SameLine();

        ImGui.TextWrapped($"Mode: {((PresetMode)preset.PositionMode).GetDescription()}");

        ImGuiUtils.IconText(FontAwesomeIcon.ExpandAlt);
        ImGui.SameLine();

        ImGui.Text($"Zoom: {preset.Distance:F2},  FoV: {preset.ZoomFoV + preset.GposeFoV:F2}");

        ImGuiUtils.IconText(FontAwesomeIcon.Expand, tooltip: "Camera Angle");
        ImGui.SameLine();

        ImGui.Text($"H: {MathUtils.RadToDeg(preset.HRotation):F2}\x00B0,  V: {MathUtils.RadToDeg(preset.VRotation):F2}\x00B0");

        ImGuiUtils.IconText(FontAwesomeIcon.ArrowsUpDownLeftRight);
        ImGui.SameLine();

        ImGui.Text($"Pan: {MathUtils.RadToDeg(preset.Pan):F0}\x00B0, ");
        ImGui.SameLine();

        ImGui.Text($"Tilt: {MathUtils.RadToDeg(preset.Tilt):F0}\x00B0, ");
        ImGui.SameLine();

        ImGui.Text($"Roll: {MathUtils.RadToDeg(preset.Roll):F0}\x00B0");

        ImGui.PopStyleColor(1);
    }

    private void DrawLightingPresetInfo()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));

        ImGuiUtils.IconText(FontAwesomeIcon.Paste);
        ImGui.SameLine();

        ImGui.TextWrapped($"Mode: {((PresetMode)this.SelectedPreset.PositionMode).GetDescription()}");

        foreach (var light in ((LightingPreset)this.SelectedPreset).Lights)
        {
            ImGui.Separator();
            if (!light.Active) { ImGui.BeginDisabled(); }

            ImGuiUtils.IconText(FontAwesomeIcon.Expand, tooltip: "Position");
            ImGui.SameLine();

            ImGui.Text($"X: {light.RelativePos.X:F2},  Y: {light.RelativePos.Y:F2},  Z: {light.RelativePos.Z:F2}");

            if (this.SelectedPreset.PositionMode == (int)PresetMode.CharacterOrientation)
            {
                ImGui.SameLine();
                ImGui.Text($"({MathUtils.RadToDeg(light.RelativeRot):F2}\x00B0)");
            }

            var color = light.RGB != Vector3.Zero ? MathUtils.ConvertFloatsTo24BitColor(light.RGB) : Vector3.Zero;
            var colorFloat = color / 256f;

            ImGuiUtils.IconText(FontAwesomeIcon.Sun, tooltip: "Light Type");
            ImGui.SameLine();

            ImGui.Text($"{3 - light.Type}  |");
            ImGui.SameLine();

            // FIXME: ImGui links all three color pickers together due to their shared name. Visual bug
            ImGui.ColorEdit3("##Light ColorEdit", ref colorFloat,
                ImGuiColorEditFlags.NoPicker | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.HDR);

            if (!light.Active) { ImGui.EndDisabled(); }
        }
        ImGui.PopStyleColor(1);
    }

    private void DrawRename()
    {
        var newName = this.SelectedPreset.Name;
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("##RenameLighting", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            this.RenameOpen = false;
            this.PresetIndex = this._presetHandler.Rename(this.SelectedPreset, newName);
            if (this.PresetIndex == -1)
            {
                Service.PluginLog.Information($"Can't rename preset \"{this.SelectedPreset.Name}\" to \"{newName}\" - Name is taken");
                this.ErrorMessage = "Names must be unique";
            }
        }
        ImGui.PopItemWidth();
    }
}
