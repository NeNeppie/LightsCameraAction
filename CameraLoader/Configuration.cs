using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Collections.Generic;
using System;

namespace CameraLoader;

public enum PresetMode
{
    Character,
    Camera
}

[Serializable]
public class cameraPreset
{
    public string name { get; set; } = "";
    public int positionMode { get; set; }

    public float distance { get; set; }
    public float hRotation { get; set; }
    public float vRotation { get; set; }
    public float zoomFoV { get; set; }  // Applies when zooming in very closely
    public float gposeFoV { get; set; } // Can be adjusted in the GPose settings menu
    public float pan { get; set; }
    public float tilt { get; set; }
    public float roll { get; set; }

    public cameraPreset() { }
    public cameraPreset(int index)
    {
        this.name = $"Preset No.{index}";
    }
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public int numOfPresets = 0;
    public List<cameraPreset> presets = new();

    [NonSerialized]
    private DalamudPluginInterface _pluginInterface;

    public void Initialize(DalamudPluginInterface pi)
    {
        this._pluginInterface = pi;
    }

    public void Save()
    {
        this._pluginInterface.SavePluginConfig(this);
    }
}
