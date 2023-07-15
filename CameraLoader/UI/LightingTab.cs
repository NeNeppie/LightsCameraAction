using System.Numerics;
using Dalamud.Interface;

using CameraLoader.Game;
using CameraLoader.Utils;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow
{
    private LightingPreset _selectedPresetL = null;

    public unsafe void DrawLightingTab()
    {
        bool res = ImGui.BeginTabItem("Lighting##LightingTab");
        if (!res) { return; }

        // Drawing here
        bool isInGPose = IsInGPose();
        if (!isInGPose) { ImGui.BeginDisabled(); }

        ImGuiUtils.IconText(FontAwesomeIcon.Lightbulb); ImGui.SameLine();
        DrawPresetModeSelection();

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 60f);
        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Yellow, size: new Vector2(60f, 60f), tooltip: "Create a new preset"))
        {
            var preset = new LightingPreset(_presetMode);
            Service.Config.LightingPresetNames.Add(preset.Name);
            Service.Config.LightingPresets.Add(preset);
            Service.Config.Save();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
        // TODO: Adjustable height based on rows
        ImGui.BeginChild("Preset Menu##Lighting", new Vector2(0.0f, 300f), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref _searchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.LightingPresets.Count; i++)
        {
            var preset = Service.Config.LightingPresets[i];
            if (!preset.Name.ToLower().Contains(_searchQuery.ToLower())) { continue; }

            bool isCurrentSelected = _selected == i;
            if (ImGui.Selectable($"{preset.Name}##Lighting", isCurrentSelected))
            {
                this._selected = isCurrentSelected ? -1 : i;
                this._selectedPresetL = isCurrentSelected ? null : preset;

                this._renameOpen = false;
                this._errorMessage = "";
            }
        }
        ImGui.EndChild();

        if (_selectedPresetL != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            ImGui.BeginChild("Preset Detail", new Vector2(0.0f, 415f), true);
            ImGui.PopStyleVar(1);

            DrawPresetInfo(ref _selectedPresetL);
            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                _selectedPresetL.Load();
            }
            ImGui.SameLine();
            if (ImGuiUtils.ColoredButton("Rename", ImGuiUtils.Orange))
            {
                this._renameOpen = !_renameOpen;
                this._errorMessage = "";
            }
            ImGui.SameLine();
            if (ImGuiUtils.ColoredButton("Remove", ImGuiUtils.Red))
            {
                RemovePreset(ref _selectedPresetL);
                this._selected = -1;
                this._selectedPresetL = null;
            }

            if (_renameOpen)
            {
                string newName = _selectedPresetL.Name;
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.InputText("##RenameLighting", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    var oldName = _selectedPresetL.Rename(newName);
                    this._renameOpen = false;

                    if (oldName != null)
                    {
                        Service.Config.LightingPresetNames.Remove(oldName);
                        Service.Config.LightingPresetNames.Add(newName);
                        Service.Config.Save();
                    }
                    else
                    {
                        this._errorMessage = "Names must be unique";
                    }
                }
                ImGui.PopItemWidth();
            }

            DrawErrorMessage();
            ImGui.EndChild();
        }

        if (!isInGPose) { ImGui.EndDisabled(); }
        ImGui.EndTabItem();
    }

    private void DrawPresetInfo(ref LightingPreset preset)
    {
        ImGui.TextWrapped(preset.Name);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));

        ImGui.TextWrapped($"Mode: {(PresetMode)preset.PositionMode} Position");
        foreach (var light in preset.Lights)
        {
            ImGui.Separator();
            ImGui.Text($"Position: {light.relativePos.ToString("F2")}");
            ImGui.Text($"Type: {light.Type}");
            ImGui.Text($"Red: {light.RGB.X:F2}, Green: {light.RGB.Y:F2}, Blue: {light.RGB.Z:F2}");
        }
        ImGui.PopStyleColor(1);
    }

    private void RemovePreset(ref LightingPreset preset)
    {
        Service.Config.LightingPresetNames.Remove(preset.Name);
        Service.Config.LightingPresets.Remove(preset);
        Service.Config.Save();
    }
}