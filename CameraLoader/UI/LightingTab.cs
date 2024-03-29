using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

using CameraLoader.Game;
using CameraLoader.Utils;

namespace CameraLoader.UI;

public class LightingTab : PresetTabBase
{
    private static float SelectionHeight => ImGui.GetFrameHeightWithSpacing() + ((ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y) * Service.Config.RowsVisibleLighting);
    private static float InfoHeight => (ImGui.GetTextLineHeightWithSpacing() * 8f) + (ImGui.GetStyle().ItemSpacing.Y * 3f) + (ImGui.GetFrameHeightWithSpacing() * 2f);

    public void Draw()
    {
        if (!ImGui.BeginTabItem("Lighting")) { return; }

        ImGui.Spacing();

        var isInGPose = this.IsInGPose();

        // Preset saving
        this.DrawModeSelection();
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGuiUtils.ButtonSizeLarge.X);

        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Yellow, ImGuiUtils.ButtonSizeLarge, "Create a new preset"))
        {
            var preset = new LightingPreset(this.SelectedMode);
            Service.Config.LightingPresetNames.Add(preset.Name);
            Service.Config.LightingPresets.Add(preset);
            Service.Config.SortPresetList(Service.Config.LightingPresets, Service.Config.SortingModeLighting);
            Service.Config.Save();
        }

        ImGui.Spacing();

        // Preset selection
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
        ImGui.BeginChild("Preset Menu##Lighting", new Vector2(0f, SelectionHeight + (ImGuiUtils.FrameRounding * 2f)), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref this.SearchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.LightingPresets.Count; i++)
        {
            var preset = Service.Config.LightingPresets[i];
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

            DrawPresetInfo();

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
                this.RemovePreset();

            if (this.RenameOpen) { DrawRename(); }

            this.DrawErrorMessage();
            ImGui.EndChild();
        }

        if (!isInGPose) { ImGui.EndDisabled(); }
        ImGui.EndTabItem();
    }

    private void DrawModeSelection()
    {
        ImGuiUtils.IconText(FontAwesomeIcon.Lightbulb);
        ImGui.SameLine();

        ImGui.Text("Preset Mode:");
        ImGui.BeginGroup();
        foreach (var mode in Enum.GetValues<PresetMode>())
        {
            ImGui.RadioButton(mode.GetDescription(), ref this.SelectedMode, (int)mode);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(mode.GetTooltip());
        }
        ImGui.EndGroup();
    }

    private void DrawPresetInfo()
    {
        ImGui.TextWrapped(this.SelectedPreset.Name);

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

            ImGuiUtils.IconText(FontAwesomeIcon.Sun, tooltip: "Light Type");
            ImGui.SameLine();

            ImGui.Text($"{3 - light.Type}  |");
            ImGui.SameLine();

            ImGui.TextColored(ImGuiColors.DPSRed, $"R: {color.X:F0}"); ImGui.SameLine(); ImGui.Text("|");
            ImGui.SameLine();

            ImGui.TextColored(ImGuiColors.HealerGreen, $"G: {color.Y:F0}"); ImGui.SameLine(); ImGui.Text("|");
            ImGui.SameLine();

            ImGui.TextColored(ImGuiColors.TankBlue, $"B: {color.Z:F0}");

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
            var oldName = this.SelectedPreset.Rename(newName);
            this.RenameOpen = false;

            if (oldName is not null)
            {
                Service.Config.LightingPresetNames.Remove(oldName);
                Service.Config.LightingPresetNames.Add(newName);
                Service.Config.SortPresetList(Service.Config.LightingPresets, Service.Config.SortingModeLighting);
                this.PresetIndex = Service.Config.LightingPresets.IndexOf((LightingPreset)this.SelectedPreset);
                Service.Config.Save();
            }
            else
                this.ErrorMessage = "Names must be unique";
        }
        ImGui.PopItemWidth();
    }

    private void RemovePreset()
    {
        Service.Config.LightingPresetNames.Remove(SelectedPreset.Name);
        Service.Config.LightingPresets.Remove((LightingPreset)SelectedPreset);
        Service.Config.Save();

        this.PresetIndex = -1;
        this.SelectedPreset = null;
    }
}
