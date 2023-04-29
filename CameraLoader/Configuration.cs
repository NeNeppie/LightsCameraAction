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
public class CameraPreset
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

    public CameraPreset() { }
    public CameraPreset(string name)
    {
        this.Name = name;
    }
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public List<CameraPreset> Presets = new();

    [NonSerialized]
    public HashSet<string> PresetNames = new();
    private DalamudPluginInterface _pluginInterface;

    public void Initialize(DalamudPluginInterface pi)
    {
        this._pluginInterface = pi;
        foreach (var preset in Presets)
        {
            PresetNames.Add(preset.Name);
        }
    }

    public void Save()
    {
        this._pluginInterface.SavePluginConfig(this);
    }
}
