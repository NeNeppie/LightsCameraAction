using System;
using Dalamud.Logging;

using CameraLoader.Game.Structs;

namespace CameraLoader.Game;

public enum PresetMode
{
    Character,
    CameraOrientation,
    CameraPosition
}

public unsafe abstract class PresetBase
{
    public string Name { get; set; } = "";
    public int PositionMode { get; set; }

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