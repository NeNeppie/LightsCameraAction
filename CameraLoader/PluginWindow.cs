using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using CameraLoader.Utils;
using System.Numerics;
using System;

using ImGuiNET;

namespace CameraLoader;

public unsafe class PluginWindow : Window
{
    private GameCamera* _camera;

    private int _renamedIndex = -1;
    private bool _renameOpen = false;

    private int _errorIndex = -1;
    private string _errorMessage = "";

    private int _primaryFocus = -1;
    private string _searchQuery = "";
    private static int _presetMode = (int)PresetMode.Character;

    public PluginWindow() : base("CameraLoader")
    {
        IsOpen = false;
        Size = new Vector2(305, 420);
        SizeCondition = ImGuiCond.FirstUseEver;

        var cameraManager = (CameraManager*)Service.SigScanner.GetStaticAddressFromSig("4C 8D 35 ?? ?? ?? ?? 85 D2");
        this._camera = cameraManager->WorldCamera;
        PluginLog.Debug($"Camera memory @ {((IntPtr)this._camera).ToString("X")}");
    }

    public override void Draw()
    {
        if (!IsOpen) { return; }

        bool isInCameraMode = Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene];
        bool gposeActorExists = Service.ObjectTable[201] != null;
        if (!(isInCameraMode && gposeActorExists))
        {
            ImGui.TextWrapped("To use the plugin you must be in Group Pose.");
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

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.8f, 0.41f, 0.7f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.9f, 0.41f, 0.7f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 1f, 0.41f, 0.7f));
        if (ImGui.Button($"Create a new preset", new Vector2(ImGui.GetContentRegionAvail().X, 40f)))
        {
            SavePreset();
        }
        ImGui.PopStyleColor(3);

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
                    if (!LoadPreset(ref preset))
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
                    DeletePreset(ref preset);

                    this._renameOpen = false;
                    this._primaryFocus = -1;
                }

                if (_renameOpen && _primaryFocus == i)
                {
                    string newName = preset.Name;
                    if (ImGui.InputText("##Rename(Input)", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        if (RenamePreset(ref preset, newName))
                        {
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

    private void SavePreset()
    {
        string presetName = "";
        for (int i = 1; i <= Service.Config.Presets.Count + 1; i++)
        {
            presetName = $"Preset #{i}";
            if (!Service.Config.PresetNames.Contains(presetName))
            {
                break;
            }
        }
        CameraPreset preset = new CameraPreset(presetName);

        float cameraRot = _camera->HRotation;
        float relativeRot = cameraRot;

        if (_presetMode == (int)PresetMode.Character)
        {
            float playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
            relativeRot = MathUtils.CameraToRelative(cameraRot, playerRot);
        }

        // First Person Mode
        if (_camera->Mode == 0) { relativeRot = MathUtils.SubPiRad(relativeRot); }

        preset.PositionMode = _presetMode;
        preset.Distance = _camera->Distance;
        preset.HRotation = relativeRot;
        preset.VRotation = _camera->VRotation;
        preset.ZoomFoV = _camera->FoV;
        preset.GposeFoV = _camera->AddedFoV;
        preset.Pan = _camera->Pan;
        preset.Tilt = _camera->Tilt;
        preset.Roll = _camera->Roll;

        Service.Config.PresetNames.Add(presetName);
        Service.Config.Presets.Add(preset);
        Service.Config.Save();
    }

    private bool isValidPreset(ref CameraPreset preset)
    {
        // Zoom check.
        // Breaks below Min distance. Doesn't go above Max, but Max can be externally modified
        if (preset.Distance < 1.5f || preset.Distance > 20f) { return false; }

        // FoV check.
        // Zoom FoV carries outside of gpose! Negative values flip the screen, High positive values are effectively a zoom hack
        // Gpose FoV resets when exiting gpose, but we don't want people suddenly entering gpose during a fight.
        if (preset.ZoomFoV < 0.69f || preset.ZoomFoV > 0.78f || preset.GposeFoV < -0.5f || preset.GposeFoV > 0.5f) { return false; }

        // Pan and Tilt check.
        // Both reset when exiting gpose, but can still be modified beyond the limits the game sets
        if (preset.Pan < -0.873f || preset.Pan > 0.873f || preset.Tilt < -0.647f || preset.Tilt > 0.342f) { return false; }

        return true;
    }

    private bool LoadPreset(ref CameraPreset preset)
    {
        if (!isValidPreset(ref preset)) { return false; }

        float hRotation = preset.HRotation;
        if (preset.PositionMode == (int)PresetMode.Character)
        {
            float playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
            hRotation = MathUtils.RelativeToCamera(preset.HRotation, playerRot);
        }

        // First Person Mode
        if (_camera->Mode == 0) { hRotation = MathUtils.AddPiRad(hRotation); }

        _camera->Distance = preset.Distance;
        _camera->HRotation = hRotation;
        _camera->VRotation = preset.VRotation;
        _camera->FoV = preset.ZoomFoV;
        _camera->AddedFoV = preset.GposeFoV;
        _camera->Pan = preset.Pan;
        _camera->Tilt = preset.Tilt;
        _camera->Roll = preset.Roll;

        return true;
    }

    private bool RenamePreset(ref CameraPreset preset, string name)
    {
        if (Service.Config.PresetNames.Contains(name))
        {
            PluginLog.Information($"Couldn't rename preset \"{preset.Name}\" to \"{name}\" - Name is taken");
            return false;
        }
        Service.Config.PresetNames.Remove(preset.Name);
        preset.Name = name;
        Service.Config.PresetNames.Add(name);
        Service.Config.Save();
        return true;
    }

    private void DeletePreset(ref CameraPreset preset)
    {
        Service.Config.PresetNames.Remove(preset.Name);
        Service.Config.Presets.Remove(preset);
        Service.Config.Save();
    }
}
