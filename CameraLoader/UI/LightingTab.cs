using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

using CameraLoader.Game;
using CameraLoader.Utils;

using ImGuiNET;
using System;

namespace CameraLoader.UI;

public class LightingTab : PresetTabBase
{
    private static float SelectionHeight => ImGui.GetFrameHeightWithSpacing() + (ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y) * Service.Config.RowsVisibleLighting;
    private static float InfoHeight => ImGui.GetTextLineHeightWithSpacing() * 11f + ImGui.GetStyle().ItemSpacing.Y * 3f + ImGui.GetFrameHeightWithSpacing() * 2f;

    public void Draw()
    {
        bool res = ImGui.BeginTabItem("Lighting");
        if (!res) { return; }

        ImGui.Spacing();

        bool isInGPose = IsInGPose();

        // Preset saving
        DrawModeSelection();
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGuiUtils.ButtonSizeLarge.X);

        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Yellow, ImGuiUtils.ButtonSizeLarge, "Create a new preset"))
        {
            var preset = new LightingPreset(SelectedMode);
            Service.Config.LightingPresetNames.Add(preset.Name);
            Service.Config.LightingPresets.Add(preset);
            Service.Config.Save();
        }

        ImGui.Spacing();

        // Preset selection
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
        ImGui.BeginChild("Preset Menu##Lighting", new Vector2(0f, SelectionHeight + ImGuiUtils.FrameRounding * 2f), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref SearchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.LightingPresets.Count; i++)
        {
            var preset = Service.Config.LightingPresets[i];
            if (!preset.Name.ToLower().Contains(SearchQuery.ToLower())) { continue; }

            bool isCurrentSelected = PresetIndex == i;
            if (ImGui.Selectable($"{preset.Name}", isCurrentSelected))
            {
                PresetIndex = isCurrentSelected ? -1 : i;
                SelectedPreset = isCurrentSelected ? null : preset;

                RenameOpen = false;
                ErrorMessage = "";
            }
        }
        ImGui.EndChild();

        // Preset information
        if (SelectedPreset is not null)
        {
            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
            ImGui.BeginChild("Preset Detail", new Vector2(0f, InfoHeight + ImGuiUtils.FrameRounding * 2f), true, ImGuiWindowFlags.NoScrollbar);
            ImGui.PopStyleVar(1);

            DrawPresetInfo();

            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                SelectedPreset.Load();
                ErrorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Rename", ImGuiUtils.Orange))
            {
                RenameOpen = !RenameOpen;
                ErrorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Remove", ImGuiUtils.Red))
                RemovePreset();

            if (RenameOpen) { DrawRename(); }

            DrawErrorMessage();
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
            ImGui.RadioButton(mode.GetDescription(), ref SelectedMode, (int)mode);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(mode.GetTooltip());
        }
        ImGui.EndGroup();
    }

    private void DrawPresetInfo()
    {
        ImGui.TextWrapped(SelectedPreset.Name);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));
        ImGui.TextWrapped($"Mode: {((PresetMode)SelectedPreset.PositionMode).GetDescription()}");

        int i = 1;
        foreach (var light in ((LightingPreset)SelectedPreset).Lights)
        {
            ImGui.Separator();
            if (!light.Active) { ImGui.BeginDisabled(); }

            ImGui.Text($"Light {i++} | Type: {3 - light.Type}");
            ImGui.Text($"Position: {light.RelativePos:F2}");

            if (SelectedPreset.PositionMode == (int)PresetMode.CharacterOrientation)
            {
                ImGui.SameLine();
                ImGui.Text($"({MathUtils.RadToDeg(light.RelativeRot):F2}\x00B0)");
            }

            var color = light.RGB != Vector3.Zero ? MathUtils.ConvertFloatsTo24BitColor(light.RGB) : Vector3.Zero;

            ImGuiUtils.IconText(FontAwesomeIcon.Circle, ImGuiColors.DPSRed); ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DPSRed, $"R: {color.X:F0}, "); ImGui.SameLine();

            ImGuiUtils.IconText(FontAwesomeIcon.Circle, ImGuiColors.HealerGreen); ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.HealerGreen, $"G: {color.Y:F0}, "); ImGui.SameLine();

            ImGuiUtils.IconText(FontAwesomeIcon.Circle, ImGuiColors.TankBlue); ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.TankBlue, $"B: {color.Z:F0}");

            if (!light.Active) { ImGui.EndDisabled(); }
        }
        ImGui.PopStyleColor(1);
    }

    private void DrawRename()
    {
        string newName = SelectedPreset.Name;
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("##RenameLighting", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            var oldName = SelectedPreset.Rename(newName);
            RenameOpen = false;

            if (oldName is not null)
            {
                Service.Config.LightingPresetNames.Remove(oldName);
                Service.Config.LightingPresetNames.Add(newName);
                Service.Config.Save();
            }
            else
                ErrorMessage = "Names must be unique";
        }
        ImGui.PopItemWidth();
    }

    private void RemovePreset()
    {
        Service.Config.LightingPresetNames.Remove(SelectedPreset.Name);
        Service.Config.LightingPresets.Remove((LightingPreset)SelectedPreset);
        Service.Config.Save();

        PresetIndex = -1;
        SelectedPreset = null;
    }
}