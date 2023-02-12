using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using System.Numerics;
using System;

using ImGuiNET;

namespace CameraLoader;

public unsafe class PluginWindow : Window
{
    private readonly ClientState _clientState;
    private readonly ObjectTable _objectTable;
    private readonly Configuration _config;
    private readonly SigScanner _sigScanner;

    private GameCamera* _camera;

    public PluginWindow(ClientState clientState, Configuration config, ObjectTable objectTable, SigScanner sigScanner) : base("CameraLoader Config")
    {
        IsOpen = false;
        Size = new Vector2(810, 520);
        SizeCondition = ImGuiCond.FirstUseEver;

        this._clientState = clientState;
        this._objectTable = objectTable;
        this._config = config;
        this._sigScanner = sigScanner;

        var cameraManager = (CameraManager*)this._sigScanner.GetStaticAddressFromSig("4C 8D 35 ?? ?? ?? ?? 85 D2");
        this._camera = cameraManager->WorldCamera;
        PluginLog.Debug($"Camera memory @ {((IntPtr)this._camera).ToString("X")}");
    }

    public override void Draw()
    {
        if (!IsOpen) { return; }

        bool isInCameraMode = this._clientState.LocalPlayer?.OnlineStatus.Id == 18;
        bool gposeActorExists = this._objectTable[201] != null;
        if (isInCameraMode && gposeActorExists)
        {
            // Save a preset
            if (ImGui.Button($"Save position"))
            {
                float cameraRot = _camera->HRotation;
                float playerRot = (float)this._clientState.LocalPlayer?.Rotation;
                float relativeRot = CameraToRelative(cameraRot, playerRot);

                cameraPreset preset = new cameraPreset(++_config.numOfPresets);
                preset.distance = _camera->Distance;
                preset.hRotation = relativeRot;
                preset.vRotation = _camera->VRotation;
                preset.zoomFoV = _camera->FoV;
                preset.gposeFoV = _camera->AddedFoV;
                preset.pan = _camera->Pan;
                preset.tilt = _camera->Tilt;
                preset.roll = _camera->Roll;

                _config.presets.Add(preset);
                _config.Save();
            }

            // TODO: Remove Preset Button

            // TODO: Rename Button, view details
            // TODO: Ability to choose between "Character Position" and "Camera Position" save/load methods

            // Load a preset
            for (int i = 0; i < _config.numOfPresets; i++)
            {
                var preset = _config.presets[i];
                if (ImGui.Selectable(preset.name, false))
                {
                    _camera->Distance = preset.distance;
                    _camera->HRotation = RelativeToCamera(preset.hRotation, (float)this._clientState.LocalPlayer?.Rotation);
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
