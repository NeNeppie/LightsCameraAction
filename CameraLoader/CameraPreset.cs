using System;
using Dalamud.Logging;

using CameraLoader.Utils;

namespace CameraLoader;

public enum PresetMode
{
    Character,
    Camera
}

public unsafe class CameraPreset
{
    public string Name { get; set; } = "";
    public int PositionMode { get; set; }

    public float Distance { get; set; }
    public float HRotation { get; set; }
    public float VRotation { get; set; }
    public float ZoomFoV { get; set; }  // Applies when zooming in very closely
    public float GposeFoV { get; set; } // Can be adjusted in the GPose settings menu
    public float Pan { get; set; }
    public float Tilt { get; set; }
    public float Roll { get; set; }

    [NonSerialized]
    private static GameCamera* _camera;

    static CameraPreset()
    {
        var cameraManager = (CameraManager*)Service.SigScanner.GetStaticAddressFromSig("4C 8D 35 ?? ?? ?? ?? 85 D2");
        _camera = cameraManager->WorldCamera;
        PluginLog.Debug($"Pointer to game camera @ {((IntPtr)_camera).ToString("X")}");
    }

    public CameraPreset() { }
    public CameraPreset(int mode = 0)
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

        float cameraRot = HRotation;
        float relativeRot = cameraRot;

        if (mode == (int)PresetMode.Character)
        {
            float playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
            relativeRot = MathUtils.CameraToRelative(cameraRot, playerRot);
        }

        // First Person Mode
        if (_camera->Mode == 0) { relativeRot = MathUtils.SubPiRad(relativeRot); }

        this.PositionMode = mode;
        this.Distance = _camera->Distance;
        this.HRotation = relativeRot;
        this.VRotation = _camera->VRotation;
        this.ZoomFoV = _camera->FoV;
        this.GposeFoV = _camera->AddedFoV;
        this.Pan = _camera->Pan;
        this.Tilt = _camera->Tilt;
        this.Roll = _camera->Roll;
    }

    public bool IsValid()
    {
        // Zoom check.
        // Breaks below Min distance. Doesn't go above Max, but Max can be externally modified
        if (Distance < 1.5f || Distance > 20f) { return false; }

        // FoV check.
        // Zoom FoV carries outside of gpose! Negative values flip the screen, High positive values are effectively a zoom hack
        // Gpose FoV resets when exiting gpose, but we don't want people suddenly entering gpose during a fight.
        if (ZoomFoV < 0.69f || ZoomFoV > 0.78f || GposeFoV < -0.5f || GposeFoV > 0.5f) { return false; }

        // Pan and Tilt check.
        // Both reset when exiting gpose, but can still be modified beyond the limits the game sets
        if (Pan < -0.873f || Pan > 0.873f || Tilt < -0.647f || Tilt > 0.342f) { return false; }

        return true;
    }

    public bool Load()
    {
        if (!IsValid()) { return false; }

        float hRotation = HRotation;
        if (PositionMode == (int)PresetMode.Character)
        {
            float playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
            hRotation = MathUtils.RelativeToCamera(HRotation, playerRot);
        }

        // First Person Mode
        if (_camera->Mode == 0) { hRotation = MathUtils.AddPiRad(hRotation); }

        _camera->Distance = Distance;
        _camera->HRotation = hRotation;
        _camera->VRotation = VRotation;
        _camera->FoV = ZoomFoV;
        _camera->AddedFoV = GposeFoV;
        _camera->Pan = Pan;
        _camera->Tilt = Tilt;
        _camera->Roll = Roll;

        return true;
    }

    public string Rename(string name)
    {
        if (Service.Config.PresetNames.Contains(name))
        {
            PluginLog.Information($"Couldn't rename preset \"{Name}\" to \"{name}\" - Name is taken");
            return null;
        }
        string oldName = Name;
        Name = name;
        return oldName;
    }
}