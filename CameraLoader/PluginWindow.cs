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

        bool isInCameraMode = Service.ClientState.LocalPlayer?.OnlineStatus.Id == 18;
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

            if (ImGui.TreeNode($"{preset.name}##{i}"))
            {
                PrintPreset(ref preset);
                if (ImGui.Button("Load Preset"))
                {
                    LoadPreset(ref preset);
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

                if (this._renameOpen && this._primaryFocus == i)
                {
                    string newName = preset.name;
                    if (ImGui.InputText("##Rename(Input)", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        preset.name = newName;
                        Service.Config.Save();
                        this._renameOpen = false;
                        this._primaryFocus = -1;
                    }
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

    private void LoadPreset(ref cameraPreset preset)
    {
        float hRotation = preset.hRotation;
        if (preset.positionMode == (int)PresetMode.Character)
        {
            hRotation = MathUtils.RelativeToCamera(preset.hRotation, (float)Service.ClientState.LocalPlayer?.Rotation);
        }

        _camera->Distance = preset.distance;
        _camera->HRotation = hRotation;
        _camera->VRotation = preset.vRotation;
        _camera->FoV = preset.zoomFoV;
        _camera->AddedFoV = preset.gposeFoV;
        _camera->Pan = preset.pan;
        _camera->Tilt = preset.tilt;
        _camera->Roll = preset.roll;
    }

    private void DeletePreset(ref cameraPreset preset)
    {
        Service.Config.presets.Remove(preset);
        Service.Config.numOfPresets--;
        Service.Config.Save();
    }
}
