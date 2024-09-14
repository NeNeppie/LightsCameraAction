using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace CameraLoader.Game;

public abstract class PresetHandler
{
    public abstract void Import(string encoded);
    public abstract void Create(string name, int mode);
    public abstract List<PresetBase> GetPresets();
    public abstract int Rename(PresetBase preset, string name);
    public abstract void Delete(PresetBase preset);

    protected static string Deserialize(string encoded)
    {
        try
        {
            var jsonBytes = Convert.FromBase64String(encoded);
            var jsonString = Encoding.UTF8.GetString(jsonBytes);
            return jsonString;
        }
        catch (Exception e)
        {
            Service.PluginLog.Error($"Error decompressing preset string: {e.StackTrace}");
            return null;
        }
    }
}

public class CameraPresetHandler : PresetHandler
{
    public override void Import(string encoded)
    {
        if (encoded == null)
            return;

        CameraPreset preset;
        try
        {
            var jsonString = Deserialize(encoded);
            preset = JsonSerializer.Deserialize<CameraPreset>(jsonString, new JsonSerializerOptions() { IncludeFields = true });
        }
        catch (Exception e)
        {
            Service.PluginLog.Error($"Error importing Camera Preset: {e.Message}\n {e.StackTrace}");
            return;
        }

        if (preset != null)
        {
            // TODO: "Imported Preset #{i}"
            //       Add Config setting to preserve/overwrite preset creation date
            preset.Name = "Imported Preset";
            //Service.Config.CameraPresetNames.Add(preset.Name);
            Service.Config.CameraPresets.Add(preset);
            Service.Config.SortPresetList(Service.Config.CameraPresets, Service.Config.SortingModeCamera);
            Service.Config.Save();
        }
    }

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
    public override void Import(string encoded)
    {
        if (encoded == null)
            return;

        LightingPreset preset;
        try
        {
            var jsonString = Deserialize(encoded);
            preset = JsonSerializer.Deserialize<LightingPreset>(jsonString, new JsonSerializerOptions() { IncludeFields = true });
        }
        catch (Exception e)
        {
            Service.PluginLog.Error($"Error importing Lighting Preset: {e.Message}\n {e.StackTrace}");
            return;
        }

        if (preset != null)
        {
            // TODO: "Imported Preset #{i}"
            //       Add Config setting to preserve/overwrite preset creation date
            preset.Name = "Imported Preset";
            //Service.Config.LightingPresetNames.Add(preset.Name);
            Service.Config.LightingPresets.Add(preset);
            Service.Config.SortPresetList(Service.Config.LightingPresets, Service.Config.SortingModeLighting);
            Service.Config.Save();
        }
    }

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