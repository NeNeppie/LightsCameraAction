using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using System.Numerics;
using System;

using ImGuiNET;

namespace CameraLoader;

public unsafe class PluginWindow : Window
{
    private GameCamera* _camera;

    private bool _renameOpen = false;
    private int _primaryFocus = -1;

    public PluginWindow() : base("CameraLoader")
    {
        IsOpen = false;
        Size = new Vector2(405, 420);
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
        if (isInCameraMode && gposeActorExists)
        {
            // Save a preset
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.8f, 0.41f, 0.7f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.9f, 0.41f, 0.7f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 1f, 0.41f, 0.7f));
            if (ImGui.Button($"Create a new preset", new Vector2(ImGui.GetContentRegionAvail().X, 40f)))
            {
                SavePreset();
            }
            ImGui.PopStyleColor(3);

            ImGui.BeginChild("Preset Menu", ImGui.GetContentRegionAvail(), true);

            for (int i = 0; i < Service.Config.numOfPresets; i++)
            {
                var preset = Service.Config.presets[i];
                if (ImGui.TreeNode($"{preset.name}##{i}"))
                {
                    ImGui.TextWrapped("Placeholder Text. A description maybe? Camera information?");
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
        else
        {
            ImGui.TextWrapped("To use the plugin you must be in Group Pose.");
        }
    }

    private void SavePreset()
    {
        float cameraRot = _camera->HRotation;
        float playerRot = (float)Service.ClientState.LocalPlayer?.Rotation;
        float relativeRot = CameraToRelative(cameraRot, playerRot);

        cameraPreset preset = new cameraPreset(++Service.Config.numOfPresets);
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
        _camera->Distance = preset.distance;
        _camera->HRotation = RelativeToCamera(preset.hRotation, (float)Service.ClientState.LocalPlayer?.Rotation);
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

    private float CameraToRelative(float camRot, float playerRot)
    {
        camRot -= playerRot;

        while (camRot > Math.PI) { camRot -= (float)Math.Tau; }
        while (camRot < -Math.PI) { camRot += (float)Math.Tau; }

        return camRot;
    }

    private float RelativeToCamera(float relRot, float playerRot)
    {
        relRot += playerRot;

        while (relRot > Math.PI) { relRot -= (float)Math.Tau; }
        while (relRot < -Math.PI) { relRot += (float)Math.Tau; }

        return relRot;
    }
}
