using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

using CameraLoader.Game;
using CameraLoader.Utils;

namespace CameraLoader.UI;

public class CameraTab : PresetTabBase
{
    private static float SelectionHeight => ImGui.GetFrameHeightWithSpacing() + ((ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y) * Service.Config.RowsVisibleCamera);
    private static float InfoHeight => (ImGui.GetTextLineHeightWithSpacing() * 5f) + (ImGui.GetFrameHeightWithSpacing() * 2f);

    public void Draw()
    {
        if (!ImGui.BeginTabItem("Camera")) { return; }

        ImGui.Spacing();

        var isInGPose = this.IsInGPose();

        // Preset saving
        this.DrawModeSelection();
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGuiUtils.ButtonSizeLarge.X);

        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Green, ImGuiUtils.ButtonSizeLarge, "Create a new preset"))
        {
            var preset = new CameraPreset(this.SelectedMode);
            Service.Config.CameraPresetNames.Add(preset.Name);
            Service.Config.CameraPresets.Add(preset);
            Service.Config.SortPresetList(Service.Config.CameraPresets, Service.Config.SortingModeCamera);
            Service.Config.Save();
        }

        ImGui.Spacing();

        // Preset selection
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
        ImGui.BeginChild("Preset Menu##Camera", new Vector2(0f, SelectionHeight + ImGuiUtils.FrameRounding * 2f), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref this.SearchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.CameraPresets.Count; i++)
        {
            var preset = Service.Config.CameraPresets[i];
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

        // Preset information
        if (this.SelectedPreset is not null)
        {
            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
            ImGui.BeginChild("Preset Detail", new Vector2(0f, InfoHeight + (ImGuiUtils.FrameRounding * 2f)), true, ImGuiWindowFlags.NoScrollbar);
            ImGui.PopStyleVar(1);

            this.DrawPresetInfo();

            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                this.ErrorMessage = "";
                if (!this.SelectedPreset.Load())
                {
                    Service.PluginLog.Information($"Attempted to load invalid Camera Preset \"{this.SelectedPreset.Name}\"");
                    this.ErrorMessage = "Preset is invalid";
                }
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Rename", ImGuiUtils.Orange))
            {
                this.RenameOpen = !this.RenameOpen;
                this.ErrorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Remove", ImGuiUtils.Red))
                this.RemovePreset();

            if (this.RenameOpen) { this.DrawRename(); }

            this.DrawErrorMessage();
            ImGui.EndChild();
        }

        if (!isInGPose) { ImGui.EndDisabled(); }
        ImGui.EndTabItem();
    }

    private void DrawModeSelection()
    {
        ImGuiUtils.IconText(FontAwesomeIcon.CameraRetro);
        ImGui.SameLine();

        ImGui.Text("Preset Mode:");
        ImGui.BeginGroup();
        foreach (var mode in Enum.GetValues<PresetMode>())
        {
            if (mode == PresetMode.CameraOrientation) continue;

            ImGui.RadioButton(mode.GetDescription(), ref this.SelectedMode, (int)mode);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(mode.GetTooltip());
        }
        ImGui.EndGroup();
    }

    private void DrawPresetInfo()
    {
        var preset = (CameraPreset)this.SelectedPreset;

        ImGui.TextWrapped(preset.Name);

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

    private void DrawRename()
    {
        var newName = this.SelectedPreset.Name;
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("##RenameCamera", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            var oldName = this.SelectedPreset.Rename(newName);
            this.RenameOpen = false;

            if (oldName is not null)
            {
                Service.Config.CameraPresetNames.Remove(oldName);
                Service.Config.CameraPresetNames.Add(newName);
                Service.Config.SortPresetList(Service.Config.CameraPresets, Service.Config.SortingModeCamera);
                this.PresetIndex = Service.Config.CameraPresets.IndexOf((CameraPreset)this.SelectedPreset);
                Service.Config.Save();
            }
            else
            {
                this.ErrorMessage = "Names must be unique";
            }
        }
        ImGui.PopItemWidth();
    }

    private void RemovePreset()
    {
        Service.Config.CameraPresetNames.Remove(SelectedPreset.Name);
        Service.Config.CameraPresets.Remove((CameraPreset)SelectedPreset);
        Service.Config.Save();

        this.PresetIndex = -1;
        this.SelectedPreset = null;
    }
}
