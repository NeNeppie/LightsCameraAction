using System.Collections.Generic;
using System.Linq;

namespace CameraLoader.Game;

public abstract class PresetHandler
{
    public abstract void Create(string name, int mode);
    public abstract List<PresetBase> GetPresets();
    public abstract int Rename(PresetBase preset, string name);
    public abstract void Delete(PresetBase preset);
}

public class CameraPresetHandler : PresetHandler
{
    public override void Create(string name, int mode)
    {
        var preset = new CameraPreset(name, mode);
        Service.Config.CameraPresetNames.Add(preset.Name);
        Service.Config.CameraPresets.Add(preset);
        Service.Config.SortPresetList(Service.Config.CameraPresets, Service.Config.SortingModeCamera);
        Service.Config.Save();
    }

    public override List<PresetBase> GetPresets() => Service.Config.CameraPresets.ToList<PresetBase>();

    public override int Rename(PresetBase preset, string name)
    {
        if (Service.Config.CameraPresetNames.Contains(name))
            return -1;

        Service.Config.CameraPresetNames.Remove(preset.Name);
        Service.Config.CameraPresetNames.Add(name);

        preset.Name = name;

        Service.Config.SortPresetList(Service.Config.CameraPresets, Service.Config.SortingModeCamera);
        Service.Config.Save();
        return GetPresets().IndexOf(preset);
    }

    public override void Delete(PresetBase preset)
    {
        Service.Config.CameraPresetNames.Remove(preset.Name);
        Service.Config.CameraPresets.Remove((CameraPreset)preset);
        Service.Config.Save();
    }
}

public class LightingPresetHandler : PresetHandler
{
    public override void Create(string name, int mode)
    {
        var preset = new LightingPreset(name, mode);
        Service.Config.LightingPresetNames.Add(preset.Name);
        Service.Config.LightingPresets.Add(preset);
        Service.Config.SortPresetList(Service.Config.LightingPresets, Service.Config.SortingModeLighting);
        Service.Config.Save();
    }

    public override List<PresetBase> GetPresets() => Service.Config.LightingPresets.ToList<PresetBase>();

    public override int Rename(PresetBase preset, string name)
    {
        if (Service.Config.LightingPresetNames.Contains(name))
            return -1;

        Service.Config.LightingPresetNames.Remove(preset.Name);
        Service.Config.LightingPresetNames.Add(name);

        preset.Name = name;

        Service.Config.SortPresetList(Service.Config.LightingPresets, Service.Config.SortingModeLighting);
        Service.Config.Save();
        return GetPresets().IndexOf(preset);
    }

    public override void Delete(PresetBase preset)
    {
        Service.Config.LightingPresetNames.Remove(preset.Name);
        Service.Config.LightingPresets.Remove((LightingPreset)preset);
        Service.Config.Save();
    }
}
