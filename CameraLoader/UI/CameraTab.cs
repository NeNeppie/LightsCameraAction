using System.Numerics;
using Dalamud.Logging;
using Dalamud.Interface;

using CameraLoader.Utils;
using CameraLoader.Game;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow
{
    private int _selected = -1;
    private bool _renameOpen = false;
    private string _errorMessage = "";
    private string _searchQuery = "";

    private CameraPreset _selectedPreset = null;

    private static int _presetMode = (int)PresetMode.Character;

    public void DrawCameraTab()
    {
        bool res = ImGui.BeginTabItem("Camera##CameraTab");
        if (!res) { return; }

        // Drawing here
        bool isInCameraMode = Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene];
        bool gposeActorExists = Service.ObjectTable[201] != null;
        if (!(isInCameraMode && gposeActorExists))
        {
            this._selectedPreset = null;
            this._selected = -1;

            ImGui.TextWrapped("Unavailable outside of Group Pose");
            ImGui.Separator();
            ImGui.BeginDisabled();
        }

        ImGuiUtils.IconText(FontAwesomeIcon.CameraRetro); ImGui.SameLine();
        ImGui.Text("Preset Mode:");
        ImGui.BeginGroup();
        {
            ImGui.RadioButton("Character Position", ref _presetMode, (int)PresetMode.Character);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Presets are saved and loaded relative to your character's orientation");
            }
            ImGui.RadioButton("Camera Position", ref _presetMode, (int)PresetMode.Camera);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Presets are saved relative to the camera's current orientation.\nYour character's orientation is not taken into account");
            }
        }
        ImGui.EndGroup();

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 60f);
        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Green, size: new Vector2(60f, 60f), tooltip: "Create a new preset"))
        {
            var preset = new CameraPreset(_presetMode);
            Service.Config.PresetNames.Add(preset.Name);
            Service.Config.Presets.Add(preset);
            Service.Config.Save();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
        // TODO: Adjustable height based on rows
        ImGui.BeginChild("Preset Menu", new Vector2(0.0f, 300f), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref _searchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.Presets.Count; i++)
        {
            var preset = Service.Config.Presets[i];
            if (!preset.Name.ToLower().Contains(_searchQuery.ToLower())) { continue; }

            bool isCurrentSelected = _selected == i;
            if (ImGui.Selectable($"{preset.Name}##Camera", isCurrentSelected))
            {
                this._selected = isCurrentSelected ? -1 : i;
                this._selectedPreset = isCurrentSelected ? null : preset;

                this._renameOpen = false;
                this._errorMessage = "";
            }
        }
        ImGui.EndChild();

        if (_selectedPreset != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            ImGui.BeginChild("Preset Detail", new Vector2(0.0f, 225f), true);
            ImGui.PopStyleVar(1);

            DrawPresetInfo(ref _selectedPreset);
            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                if (!_selectedPreset.Load())
                {
                    PluginLog.Information($"Attempted to load camera preset \"{_selectedPreset.Name}\" which contains invalid values");
                    this._errorMessage = "Error: Preset is invalid";
                }
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
                RemovePreset(ref _selectedPreset);
                this._selected = -1;
                this._selectedPreset = null;
            }

            if (_renameOpen)
            {
                string newName = _selectedPreset.Name;
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.InputText("##RenameCamera", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    var oldName = _selectedPreset.Rename(newName);
                    this._renameOpen = false;

                    if (oldName != null)
                    {
                        Service.Config.PresetNames.Remove(oldName);
                        Service.Config.PresetNames.Add(newName);
                        Service.Config.Save();
                    }
                    else
                    {
                        this._errorMessage = "Names must be unique";
                    }
                }
                ImGui.PopItemWidth();
            }

            if (_errorMessage != "")
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), _errorMessage);
            }
            ImGui.EndChild();
        }

        if (!(isInCameraMode && gposeActorExists))
        {
            ImGui.EndDisabled();
        }

        ImGui.EndTabItem();
    }

    private void DrawPresetInfo(ref CameraPreset preset)
    {
        string foVFormatted = preset.GposeFoV > 0 ? $"({preset.ZoomFoV:F2}+{preset.GposeFoV})" : $"({preset.ZoomFoV:F2}{preset.GposeFoV})";

        ImGui.TextWrapped(preset.Name);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));

        ImGui.TextWrapped($"Mode: {(PresetMode)preset.PositionMode} Position");
        ImGui.Text($"Zoom: {preset.Distance:F2} , FoV: {(preset.ZoomFoV + preset.GposeFoV):F2} " + foVFormatted);
        ImGui.Text($"H: {MathUtils.RadToDeg(preset.HRotation):F2}\x00B0 , V: {MathUtils.RadToDeg(preset.VRotation):F2}\x00B0");

        ImGui.Text($"Pan: {MathUtils.RadToDeg(preset.Pan):F0}\x00B0 , "); ImGui.SameLine();
        ImGui.Text($"Tilt: {MathUtils.RadToDeg(preset.Tilt):F0}\x00B0 , "); ImGui.SameLine();
        ImGui.Text($"Roll: {MathUtils.RadToDeg(preset.Roll):F0}\x00B0");

        ImGui.PopStyleColor(1);
    }

    private void RemovePreset(ref CameraPreset preset)
    {
        Service.Config.PresetNames.Remove(preset.Name);
        Service.Config.Presets.Remove(preset);
        Service.Config.Save();
    }
}