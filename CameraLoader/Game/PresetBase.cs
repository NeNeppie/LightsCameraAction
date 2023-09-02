using System;
using Dalamud.Logging;

using CameraLoader.Game.Structs;

namespace CameraLoader.Game;

public enum PresetMode
{
    CharacterOrientation,
    CharacterPosition,
    CameraOrientation
}

public unsafe abstract class PresetBase
{
    public string Name { get; set; } = "";
    public int PositionMode { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;

    [NonSerialized]
    protected static GameCamera* _camera;

    static PresetBase()
    {
        var cameraManager = (CameraManager*)Service.SigScanner.GetStaticAddressFromSig("4C 8D 35 ?? ?? ?? ?? 85 D2");
        _camera = cameraManager->WorldCamera;
        PluginLog.Debug($"Pointer to game camera @ 0x{(IntPtr)_camera:X16}");
    }

    public abstract bool Load();

    public abstract string Rename(string name);
}

static class PresetModeEx
{
    public static string GetDescription(this PresetMode value)
    {
        return value switch
        {
            PresetMode.CharacterOrientation => "Character Orientation",
            PresetMode.CharacterPosition => "Character Position",
            PresetMode.CameraOrientation => "Camera Orientation",
            _ => value.ToString(),
        };
    }

    public static string GetTooltip(this PresetMode value)
    {
        return value switch
        {
            PresetMode.CharacterOrientation => "Presets are saved relative to your character's position and orientation.\nThis is equivalent to the \"Character Position\" setting in-game.",
            PresetMode.CharacterPosition => "Presets are saved relative only to your character's position.\nThis is equivalent to the \"Camera Position\" setting in-game.",
            PresetMode.CameraOrientation => "Presets are saved relative to the camera's position and orientation.",
            _ => value.ToString() + "(Hello!)",
        };
    }
}