using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using System.Numerics;
using System;

using ImGuiNET;

namespace CameraLoader;

public unsafe class PluginWindow : Window
{
    private GameCamera* _camera;

    public PluginWindow() : base("CameraLoader")
    {
        IsOpen = false;
        Size = new Vector2(810, 520);
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
            if (ImGui.Button($"Save position"))
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

            // TODO: Remove Preset Button

            // TODO: Rename Button, view details
            // TODO: Ability to choose between "Character Position" and "Camera Position" save/load methods

            // Load a preset
            for (int i = 0; i < Service.Config.numOfPresets; i++)
            {
                var preset = Service.Config.presets[i];
                if (ImGui.Selectable(preset.name, false))
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
            }
        }
        else
        {
            ImGui.Text("To use the plugin you must be in Group Pose.");
        }
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
