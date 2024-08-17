using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;

using CameraLoader.Game;
using CameraLoader.Utils;

namespace CameraLoader.UI;

public class PresetTab
{
    private readonly string _tabName;
    private readonly int _modeCount;
    private readonly PresetHandler _presetHandler;
    private readonly FontAwesomeIcon _presetIcon;
    private readonly Vector4 _presetButtonColor;
    private readonly Action _presetInfoDrawFunc;

    private string _creationName = "";
    private string _errorMessage = "";
    private string _searchQuery = "";

    private PresetBase _selectedPreset = null;
    private int _selectedPresetMode = (int)PresetMode.CharacterOrientation;
    private int _selectedPresetIndex = -1;
    private bool _isRenameOpen = false;

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

        var isInGPose = IsInGPose();
        if (!isInGPose)
        {
            this._selectedPresetIndex = -1;
            this._selectedPreset = null;

            ImGui.TextWrapped("Unavailable outside of Group Pose");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginDisabled();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
        ImGui.BeginChild($"Preset Menu##{this._tabName}", new Vector2(0f, selectBoxHeight + (ImGuiUtils.FrameRounding * 2f)), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - ImGui.GetFrameHeightWithSpacing());
        ImGui.InputTextWithHint("##Search", "Search...", ref this._searchQuery, 30);
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
        if (ImGui.BeginPopup("Preset Menu##Create"))
        {
            DrawModeSelection(this._modeCount);

            ImGui.InputTextWithHint("##Preset Create Input", "Preset Name", ref this._creationName, 30);
            if (ImGui.Button("Create Preset") && this._creationName.Length > 0)
            {
                this._presetHandler.Create(this._creationName, this._selectedPresetMode);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }

        // Preset Listing
        for (int i = 0; i < this._presetHandler.GetPresets().Count; i++)
        {
            var preset = this._presetHandler.GetPresets()[i];
            if (!preset.Name.ToLower().Contains(this._searchQuery.ToLower())) { continue; }

            var isCurrentSelected = this._selectedPresetIndex == i;
            if (ImGui.Selectable($"{preset.Name}", isCurrentSelected))
            {
                this._selectedPresetIndex = isCurrentSelected ? -1 : i;
                this._selectedPreset = isCurrentSelected ? null : preset;

                this._isRenameOpen = false;
                this._errorMessage = "";
            }
        }
        ImGui.EndChild();

        // Preset Information
        if (this._selectedPreset is not null)
        {
            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
            ImGui.BeginChild("Preset Detail", new Vector2(0f, infoBoxHeight + (ImGuiUtils.FrameRounding * 2f)), true, ImGuiWindowFlags.NoScrollbar);
            ImGui.PopStyleVar(1);

            ImGui.TextWrapped(this._selectedPreset.Name);
            this._presetInfoDrawFunc();

            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                this._selectedPreset.Load();
                this._errorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Rename", ImGuiUtils.Orange))
            {
                this._isRenameOpen = !this._isRenameOpen;
                this._errorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Remove", ImGuiUtils.Red))
            {
                this._presetHandler.Delete(this._selectedPreset);
                this._selectedPresetIndex = -1;
                this._selectedPreset = null;
            }

            if (this._isRenameOpen)
                DrawRename();

            this.DrawErrorMessage();
            ImGui.EndChild();
        }

        if (!isInGPose) { ImGui.EndDisabled(); }
        ImGui.EndTabItem();
    }

    private static bool IsInGPose()
    {
        var isInCameraMode = Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene];
        var gposeActorExists = Service.ObjectTable[201] != null;
        if (!(isInCameraMode && gposeActorExists))
            return false;
        return true;
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

            ImGui.RadioButton(mode.GetDescription(), ref this._selectedPresetMode, (int)mode);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(mode.GetTooltip());
        }
        ImGui.EndGroup();
    }

    private void DrawCamersPresetInfo()
    {
        var preset = (CameraPreset)this._selectedPreset;
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

        ImGui.TextWrapped($"Mode: {((PresetMode)this._selectedPreset.PositionMode).GetDescription()}");

        for (int i = 0; i < 3; i++)
        {
            var light = ((LightingPreset)this._selectedPreset).Lights[i];
            ImGui.Separator();
            if (!light.Active) { ImGui.BeginDisabled(); }

            ImGuiUtils.IconText(FontAwesomeIcon.Expand, tooltip: "Position");
            ImGui.SameLine();

            ImGui.Text($"X: {light.RelativePos.X:F2},  Y: {light.RelativePos.Y:F2},  Z: {light.RelativePos.Z:F2}");

            if (this._selectedPreset.PositionMode == (int)PresetMode.CharacterOrientation)
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

            ImGui.ColorEdit3($"##Light ColorEdit{i}", ref colorFloat,
                ImGuiColorEditFlags.NoPicker | ImGuiColorEditFlags.NoBorder | ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.HDR);

            if (!light.Active) { ImGui.EndDisabled(); }
        }
        ImGui.PopStyleColor(1);
    }

    private void DrawRename()
    {
        var newName = this._selectedPreset.Name;
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("##RenameLighting", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            this._isRenameOpen = false;
            this._selectedPresetIndex = this._presetHandler.Rename(this._selectedPreset, newName);
            if (this._selectedPresetIndex == -1)
            {
                Service.PluginLog.Information($"Can't rename preset \"{this._selectedPreset.Name}\" to \"{newName}\" - Name is taken");
                this._errorMessage = "Names must be unique";
            }
        }
        ImGui.PopItemWidth();
    }

    private void DrawErrorMessage()
    {
        if (this._errorMessage != "")
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), this._errorMessage);
        }
    }
}
