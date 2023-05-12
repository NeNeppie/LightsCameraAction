using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace CameraLoader;

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
