using System.Numerics;
using System.Runtime.InteropServices;

using CameraLoader.Game.Structs;
using CameraLoader.Utils;

namespace CameraLoader.Game;

public class LightInfo
{
    public bool Active { get; set; }
    public float RelativeRot { get; set; }
    public Vector3 RelativePos { get; set; }
    public Vector3 RGB { get; set; }
    public byte Type { get; set; }
}

public unsafe class LightingPreset : PresetBase
{
    public LightInfo[] Lights = new LightInfo[3];

    public LightingPreset() { }
    public LightingPreset(int mode = 0)
    {
        this.Name = "";
        for (int i = 1; i <= Service.Config.LightingPresets.Count + 1; i++)
        {
            this.Name = $"Preset #{i}";
            if (!Service.Config.LightingPresetNames.Contains(Name)) { break; }
        }

        var eventFramework = FFXIVClientStructs.FFXIV.Client.Game.Event.EventFramework.Instance();
        var eventGPoseController = &eventFramework->EventSceneModule.EventGPoseController;
        for (int i = 0; i < 3; i++)
        {
            this.Lights[i] = new();

            var lightDrawObject = (LightObject*)Marshal.ReadIntPtr((nint)eventGPoseController + 0xE0 + (8 * i));
            if (lightDrawObject is null) { continue; }

            var relativeObjectPos = mode == (int)PresetMode.CameraOrientation ? _camera->Position : Service.ClientState.LocalPlayer?.Position ?? new(0, 0, 0);
            var relativeObjectRot = mode == (int)PresetMode.CameraOrientation ? _camera->HRotation - 1.5707f : Service.ClientState.LocalPlayer?.Rotation ?? 0f;

            var relativePos = lightDrawObject->Position - relativeObjectPos;
            if (mode == (int)PresetMode.CharacterOrientation || mode == (int)PresetMode.CameraOrientation)
            {
                var theta = MathUtils.ConvertToRelative(MathUtils.GetHorizontalRotation(relativePos), relativeObjectRot);
                this.Lights[i].RelativeRot = theta; // For display only
                (relativePos.X, relativePos.Z) = MathUtils.RotatePoint2D((relativePos.X, relativePos.Z), relativeObjectRot);
            }

            this.Lights[i].Active = true;
            this.Lights[i].RelativePos = relativePos;
            this.Lights[i].RGB = lightDrawObject->LightRenderObject->RGB;
            this.Lights[i].Type = lightDrawObject->LightRenderObject->Type;
        }
        this.PositionMode = mode;
    }

    public override bool Load()
    {
        var eventFramework = FFXIVClientStructs.FFXIV.Client.Game.Event.EventFramework.Instance();
        var eventGPoseController = &eventFramework->EventSceneModule.EventGPoseController;
        for (int i = 0; i < 3; i++)
        {
            var lightDrawObject = (LightObject*)Marshal.ReadIntPtr((nint)eventGPoseController + 0xE0 + (8 * i));
            if ((this.Lights[i].Active && lightDrawObject is null) ||
                (!this.Lights[i].Active && lightDrawObject is not null))
            {
                Service.GameFunctions.ToggleLight(eventGPoseController, (uint)i);
                lightDrawObject = (LightObject*)Marshal.ReadIntPtr((nint)eventGPoseController + 0xE0 + (8 * i));
            }

            if (lightDrawObject is null) { continue; }

            var relativeObjectPos = this.PositionMode == (int)PresetMode.CameraOrientation ? _camera->Position : Service.ClientState.LocalPlayer?.Position ?? new(0, 0, 0);
            var relativeObjectRot = this.PositionMode == (int)PresetMode.CameraOrientation ? _camera->HRotation - 1.5707f : Service.ClientState.LocalPlayer?.Rotation ?? 0f;

            var relativePos = this.Lights[i].RelativePos;
            if (this.PositionMode is ((int)PresetMode.CharacterOrientation) or ((int)PresetMode.CameraOrientation))
            {
                (relativePos.X, relativePos.Z) = MathUtils.RotatePoint2D((relativePos.X, relativePos.Z), relativeObjectRot);
            }

            lightDrawObject->Position = relativePos + relativeObjectPos;
            lightDrawObject->LightRenderObject->RGB = this.Lights[i].RGB;
            lightDrawObject->LightRenderObject->Type = this.Lights[i].Type;
            Service.GameFunctions.UpdateLightObject(lightDrawObject);
        }
        return true;
    }

    public override string Rename(string name)
    {
        if (Service.Config.LightingPresetNames.Contains(name))
        {
            Service.PluginLog.Information($"Couldn't rename lighting preset \"{this.Name}\" to \"{name}\" - Name is taken");
            return null;
        }
        var oldName = this.Name;
        this.Name = name;
        return oldName;
    }
}
