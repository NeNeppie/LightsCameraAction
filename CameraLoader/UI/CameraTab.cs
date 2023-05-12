using System;
using System.Numerics;
using CameraLoader.Utils;
using Dalamud.Logging;

using ImGuiNET;

namespace CameraLoader.UI;

// Draw()
// DrawCompact()
// PrintPreset()
// RemovePreset()

public partial class PluginWindow
{
    private int _renamedIndex = -1;
    private bool _renameOpen = false;

    private int _errorIndex = -1;
    private string _errorMessage = "";

    private int _primaryFocus = -1;
    private string _searchQuery = "";
    private static int _presetMode = (int)PresetMode.Character;

    public void DrawCameraTab()
    {
        // Drawing here
        bool isInCameraMode = Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene];
        bool gposeActorExists = Service.ObjectTable[201] != null;
        if (!(isInCameraMode && gposeActorExists))
        {
            ImGui.TextWrapped("Unavailable outside of Group Pose");
            return;
        }

        ImGui.Text("Preset Mode:");
        ImGui.RadioButton("Character Position", ref _presetMode, (int)PresetMode.Character); ImGui.SameLine();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Presets are saved and loaded relative to your character's orientation");
        }
        ImGui.RadioButton("Camera Position", ref _presetMode, (int)PresetMode.Camera);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Presets are saved relative to the camera's current orientation.\nYour character's orientation is not taken into account");
        }

        if (ImGuiUtils.ColoredButton($"Create a new preset", ImGuiUtils.Green, new Vector2(ImGui.GetContentRegionAvail().X, 40f)))
        {
            var preset = new CameraPreset(_presetMode);
            Service.Config.PresetNames.Add(preset.Name);
            Service.Config.Presets.Add(preset);
            Service.Config.Save();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
        ImGui.BeginChild("Preset Menu", ImGui.GetContentRegionAvail(), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref _searchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.Presets.Count; i++)
        {
            var preset = Service.Config.Presets[i];
            if (!preset.Name.ToLower().Contains(_searchQuery.ToLower())) { continue; }

            if (_renamedIndex == i)
            {
                ImGui.SetNextItemOpen(true);
                this._primaryFocus = -1;
                this._renamedIndex = _primaryFocus;
            }

            if (ImGui.TreeNode($"{preset.Name}##{i}"))
            {
                PrintPreset(ref preset);
                if (ImGui.Button("Load Preset"))
                {
                    if (!preset.Load())
                    {
                        PluginLog.Information($"Attempted to load an invalid preset \"{preset.Name}\"");
                        this._errorIndex = i;
                        this._errorMessage = "Error: Preset contains invalid values";
                    }
                }
                ImGui.SameLine();
                // Show / Hide the rename input box
                if (ImGui.Button("Rename"))
                {
                    if (this._primaryFocus != i)
                    {
                        this._renameOpen = true;
                        this._primaryFocus = i;
                    }
                    else
                    {
                        this._renameOpen = false;
                        this._primaryFocus = -1;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Delete Preset"))
                {
                    RemovePreset(ref preset);

                    this._renameOpen = false;
                    this._primaryFocus = -1;
                }

                if (_renameOpen && _primaryFocus == i)
                {
                    string newName = preset.Name;
                    if (ImGui.InputText("##Rename(Input)", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        var oldName = preset.Rename(newName);
                        if (oldName != null)
                        {
                            Service.Config.PresetNames.Remove(oldName);
                            Service.Config.PresetNames.Add(newName);
                            Service.Config.Save();

                            this._renameOpen = false;
                            this._renamedIndex = _primaryFocus;
                            this._errorIndex = -1;
                        }
                        else
                        {
                            this._errorIndex = i;
                            this._errorMessage = "Names must be unique.";
                        }
                    }
                }

                if (_errorIndex == i)
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), _errorMessage);
                }
                ImGui.TreePop();
            }
        }
        ImGui.EndChild();
        ImGui.EndTabItem();
    }

    private void PrintPreset(ref CameraPreset preset)
    {
        string foVFormatted = preset.GposeFoV > 0 ? $"({preset.ZoomFoV:F2}+{preset.GposeFoV})" : $"({preset.ZoomFoV:F2}{preset.GposeFoV})";

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));

        ImGui.TextWrapped($"Mode: {(PresetMode)preset.PositionMode} Position");
        ImGui.Text($"Zoom: {preset.Distance} , FoV: {(preset.ZoomFoV + preset.GposeFoV):F2} " + foVFormatted);
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