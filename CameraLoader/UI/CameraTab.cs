using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;

using CameraLoader.Game;
using CameraLoader.Utils;

using ImGuiNET;

namespace CameraLoader.UI;

public class CameraTab : PresetTabBase
{
    private static float SelectionHeight => ImGui.GetFrameHeightWithSpacing() + (ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y) * Service.Config.RowsVisibleCamera;
    private static float InfoHeight => ImGui.GetTextLineHeightWithSpacing() * 5f + ImGui.GetFrameHeightWithSpacing() * 2f;

    public void Draw()
    {
        bool res = ImGui.BeginTabItem("Camera");
        if (!res) { return; }

        ImGui.Spacing();

        bool isInGPose = IsInGPose();

        // Preset saving
        DrawModeSelection();
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGuiUtils.ButtonSizeLarge.X);

        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Green, ImGuiUtils.ButtonSizeLarge, "Create a new preset"))
        {
            var preset = new CameraPreset(SelectedMode);
            Service.Config.CameraPresetNames.Add(preset.Name);
            Service.Config.CameraPresets.Add(preset);
            Service.Config.Save();
        }

        ImGui.Spacing();

        // Preset selection
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, ImGuiUtils.FrameRounding);
        ImGui.BeginChild("Preset Menu##Camera", new Vector2(0f, SelectionHeight + ImGuiUtils.FrameRounding * 2f), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref SearchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.CameraPresets.Count; i++)
        {
            var preset = Service.Config.CameraPresets[i];
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
                ErrorMessage = "";
                if (!SelectedPreset.Load())
                {
                    PluginLog.Information($"Attempted to load invalid Camera Preset \"{SelectedPreset.Name}\"");
                    ErrorMessage = "Preset is invalid";
                }
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
        ImGuiUtils.IconText(FontAwesomeIcon.CameraRetro);
        ImGui.SameLine();

        ImGui.Text("Preset Mode:");
        ImGui.BeginGroup();
        {
            ImGui.RadioButton("Character Position", ref SelectedMode, (int)PresetMode.Character);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Presets are saved and loaded relative to your character's orientation and position.");

            ImGui.RadioButton("Camera Orientation", ref SelectedMode, (int)PresetMode.CameraOrientation);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Presets are saved relative to the camera's orientation. This is equivalent to the \"Camera Position\" setting in-game.");
        }
        ImGui.EndGroup();
    }

    private void DrawPresetInfo()
    {
        var preset = (CameraPreset)SelectedPreset;
        // FIXME: formatting bug when GposeFoV is 0
        string fov = preset.GposeFoV > 0 ? $"({preset.ZoomFoV:F2}+{preset.GposeFoV})" : $"({preset.ZoomFoV:F2}{preset.GposeFoV:F2})";

        ImGui.TextWrapped(preset.Name);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));
        ImGui.TextWrapped($"Mode: {(PresetMode)preset.PositionMode}");  // FIXME: Formatting Error

        ImGui.Text($"Zoom: {preset.Distance:F2} , FoV: {preset.ZoomFoV + preset.GposeFoV:F2} " + fov);
        ImGui.Text($"H: {MathUtils.RadToDeg(preset.HRotation):F2}\x00B0 , V: {MathUtils.RadToDeg(preset.VRotation):F2}\x00B0");

        ImGui.Text($"Pan: {MathUtils.RadToDeg(preset.Pan):F0}\x00B0 , ");
        ImGui.SameLine();

        ImGui.Text($"Tilt: {MathUtils.RadToDeg(preset.Tilt):F0}\x00B0 , ");
        ImGui.SameLine();

        ImGui.Text($"Roll: {MathUtils.RadToDeg(preset.Roll):F0}\x00B0");

        ImGui.PopStyleColor(1);
    }

    private void DrawRename()
    {
        string newName = SelectedPreset.Name;
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("##RenameCamera", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            var oldName = SelectedPreset.Rename(newName);
            RenameOpen = false;

            if (oldName is not null)
            {
                Service.Config.CameraPresetNames.Remove(oldName);
                Service.Config.CameraPresetNames.Add(newName);
                Service.Config.Save();
            }
            else
            {
                ErrorMessage = "Names must be unique";
            }
        }
        ImGui.PopItemWidth();
    }

    private void RemovePreset()
    {
        Service.Config.CameraPresetNames.Remove(SelectedPreset.Name);
        Service.Config.CameraPresets.Remove((CameraPreset)SelectedPreset);
        Service.Config.Save();

        PresetIndex = -1;
        SelectedPreset = null;
    }
}