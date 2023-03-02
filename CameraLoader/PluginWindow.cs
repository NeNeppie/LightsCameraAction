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

    private bool _renameOpen = false;
    private int _renamedIndex = -1;
    private int _invalidIndex = -1;
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
        ImGui.RadioButton("Camera Position", ref _presetMode, (int)PresetMode.Camera);

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

        for (int i = 0; i < Service.Config.numOfPresets; i++)
        {
            var preset = Service.Config.presets[i];
            if (!preset.name.ToLower().Contains(_searchQuery.ToLower())) { continue; }

            if (_renamedIndex == i)
            {
                ImGui.SetNextItemOpen(true);
                this._primaryFocus = -1;
                this._renamedIndex = _primaryFocus;
            }

            if (ImGui.TreeNode($"{preset.name}##{i}"))
            {
                PrintPreset(ref preset);
                if (ImGui.Button("Load Preset"))
                {
                    if (!LoadPreset(ref preset))
                    {
                        PluginLog.Information($"Attempted to load an invalid preset \"{preset.name}\"");
                        this._invalidIndex = i;
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
                    string newName = preset.name;
                    if (ImGui.InputText("##Rename(Input)", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        preset.name = newName;
                        Service.Config.Save();
                        this._renameOpen = false;
                        this._renamedIndex = _primaryFocus;
                    }
                }

                if (_invalidIndex == i)
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), "Error: Preset contains invalid values");
                }
                ImGui.TreePop();
            }
        }
        ImGui.EndChild();
    }

    private void PrintPreset(ref cameraPreset preset)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));
        ImGui.TextWrapped($"Mode: {(PresetMode)preset.positionMode} Position");
        ImGui.Text($"Zoom: {preset.distance} , FoV: {preset.zoomFoV + preset.gposeFoV:F3}");
        ImGui.Text($"H: {MathUtils.RadToDeg(preset.hRotation):F2}\x00B0 , V: {MathUtils.RadToDeg(preset.vRotation):F2}\x00B0");

        ImGui.Text($"Pan: {MathUtils.RadToDeg(preset.pan):F0}\x00B0 , "); ImGui.SameLine();
        ImGui.Text($"Tilt: {MathUtils.RadToDeg(preset.tilt):F0}\x00B0 , "); ImGui.SameLine();
        ImGui.Text($"Roll: {MathUtils.RadToDeg(preset.roll):F0}\x00B0");

        ImGui.PopStyleColor(1);
    }

    private void SavePreset()
    {
        cameraPreset preset = new cameraPreset(++Service.Config.numOfPresets);

        float cameraRot = _camera->HRotation;
        float relativeRot = cameraRot;

        if (_presetMode == (int)PresetMode.Character)
        {
            float playerRot = (float)Service.ClientState.LocalPlayer?.Rotation;
            relativeRot = MathUtils.CameraToRelative(cameraRot, playerRot);
        }

        // First Person Mode
        if (_camera->Mode == 0) { relativeRot = MathUtils.SubPiRad(relativeRot); }

        preset.positionMode = _presetMode;
        preset.distance = _camera->Distance;
        preset.hRotation = relativeRot;
        preset.vRotation = _camera->VRotation;
        preset.zoomFoV = _camera->FoV;
        preset.gposeFoV = _camera->AddedFoV;
        preset.pan = _camera->Pan;
        preset.tilt = _camera->Tilt;
        preset.roll = _camera->Roll;

        Service.Config.presets.Add(preset);
        Service.Config.Save();
    }

    private bool isValidPreset(ref cameraPreset preset)
    {
        // Zoom check.
        // Breaks below Min distance. Doesn't go above Max, but Max can be externally modified
        if (preset.distance < 1.5 || preset.distance > 20) { return false; }

        // FoV check.
        // Zoom FoV carries outside of gpose! Negative values flip the screen, High positive values are effectively a zoom hack
        // Gpose FoV resets when exiting gpose, but we don't want people suddenly entering gpose during a fight.
        if (preset.zoomFoV < 0.69 || preset.zoomFoV > 0.78 || preset.gposeFoV < -0.5 || preset.gposeFoV > 0.5) { return false; }

        // Pan and Tilt check.
        // Both reset when exiting gpose, but can still be modified beyond the limits the game sets
        if (preset.pan < -0.873 || preset.pan > 0.873 || preset.tilt < -0.647 || preset.tilt > 0.342) { return false; }

        return true;
    }

    private bool LoadPreset(ref cameraPreset preset)
    {
        if (!isValidPreset(ref preset)) { return false; }

        float hRotation = preset.hRotation;
        if (preset.positionMode == (int)PresetMode.Character)
        {
            hRotation = MathUtils.RelativeToCamera(preset.hRotation, (float)Service.ClientState.LocalPlayer?.Rotation);
        }

        // First Person Mode
        if (_camera->Mode == 0) { hRotation = MathUtils.AddPiRad(hRotation); }

        _camera->Distance = preset.distance;
        _camera->HRotation = hRotation;
        _camera->VRotation = preset.vRotation;
        _camera->FoV = preset.zoomFoV;
        _camera->AddedFoV = preset.gposeFoV;
        _camera->Pan = preset.pan;
        _camera->Tilt = preset.tilt;
        _camera->Roll = preset.roll;

        return true;
    }

    private void DeletePreset(ref cameraPreset preset)
    {
        Service.Config.presets.Remove(preset);
        Service.Config.numOfPresets--;
        Service.Config.Save();
    }
}
