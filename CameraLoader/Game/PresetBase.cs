using System;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

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
        CameraManager* cameraManager = CameraManager.Instance();
        _camera = (GameCamera*)cameraManager->Camera;
    }

    public abstract bool Load();

    public abstract string Rename(string name);
}

internal static class PresetModeEx
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
