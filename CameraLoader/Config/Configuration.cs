using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

using CameraLoader.Game;

namespace CameraLoader.Config;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool LockWindowPosition = false;
    public bool LockWindowSize = false;
    public WindowOpenMode WindowOpenMode = 0;

    public int RowsVisibleCamera = 5;
    public PresetSortingMode SortingModeCamera = 0;
    public int RowsVisibleLighting = 5;
    public PresetSortingMode SortingModeLighting = 0;

    public List<CameraPreset> CameraPresets = new();
    public List<LightingPreset> LightingPresets = new();

    [NonSerialized]
    public HashSet<string> CameraPresetNames = new();
    [NonSerialized]
    public HashSet<string> LightingPresetNames = new();
    private DalamudPluginInterface _pluginInterface;

    public void Initialize(DalamudPluginInterface pi)
    {
        this._pluginInterface = pi;
        foreach (var preset in CameraPresets)
        {
            CameraPresetNames.Add(preset.Name);
        }
        foreach (var preset in LightingPresets)
        {
            LightingPresetNames.Add(preset.Name);
        }
    }

    public void Save()
    {
        this._pluginInterface.SavePluginConfig(this);
    }

    public void SortPresetList<T>(List<T> list, PresetSortingMode mode) where T : PresetBase
    {
        switch (mode)
        {
            case PresetSortingMode.CreationDate:
                list.Sort((x, y) => DateTime.Compare(x.CreationDate, y.CreationDate));
                break;
            case PresetSortingMode.NameAscend:
                list.Sort((x, y) => string.Compare(x.Name, y.Name));
                break;
            case PresetSortingMode.NameDescend:
                list.Sort((x, y) => string.Compare(y.Name, x.Name));
                break;
        }
    }
}