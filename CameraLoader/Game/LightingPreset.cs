using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Logging;

using CameraLoader.Game.Structs;
using CameraLoader.Utils;

namespace CameraLoader.Game;

public class LightInfo
{
    public bool Active { get; set; }
    public float relativeRot { get; set; }
    public Vector3 relativePos { get; set; }
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

            DrawObject* lightDrawObject = (DrawObject*)Marshal.ReadIntPtr((nint)eventGPoseController + 0xE0 + (8 * i));
            if (lightDrawObject == null) { continue; }

            var relativePos = lightDrawObject->Position - (Service.ClientState.LocalPlayer?.Position ?? new Vector3(0, 0, 0));
            if (mode == (int)PresetMode.Character)
            {
                var playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
                var theta = MathUtils.ConvertToRelative(MathUtils.GetHorizontalRotation(relativePos), playerRot);
                this.Lights[i].relativeRot = theta; // For display only
                (relativePos.X, relativePos.Z) = MathUtils.RotatePoint2D((relativePos.X, relativePos.Z), playerRot);
            }

            this.Lights[i].Active = true;
            this.Lights[i].relativePos = relativePos;
            this.Lights[i].RGB = lightDrawObject->LightObject->RGB;
            this.Lights[i].Type = lightDrawObject->LightObject->Type;
        }
        this.PositionMode = mode;
    }

    // TODO: Implement construction/destruction of lights. 
    //       Reflect changes in the UI.
    public override bool Load()
    {
        var eventFramework = FFXIVClientStructs.FFXIV.Client.Game.Event.EventFramework.Instance();
        var eventGPoseController = &eventFramework->EventSceneModule.EventGPoseController;
        for (int i = 0; i < 3; i++)
        {
            if (!Lights[i].Active) { continue; }

            DrawObject* lightDrawObject = (DrawObject*)Marshal.ReadIntPtr((nint)eventGPoseController + 0xE0 + (8 * i));
            if (lightDrawObject == null) { continue; }

            var relativePos = Lights[i].relativePos;
            if (PositionMode == (int)PresetMode.Character)
            {
                var playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
                (relativePos.X, relativePos.Z) = MathUtils.RotatePoint2D((relativePos.X, relativePos.Z), playerRot);
            }

            lightDrawObject->Position = relativePos + (Service.ClientState.LocalPlayer?.Position ?? new Vector3(0, 0, 0));
            lightDrawObject->LightObject->RGB = Lights[i].RGB;
            // FIXME: Cutoff distance needs to be adjusted as well.
            lightDrawObject->LightObject->Type = Lights[i].Type;
        }
        return true;
    }

    public override string Rename(string name)
    {
        if (Service.Config.LightingPresetNames.Contains(name))
        {
            PluginLog.Information($"Couldn't rename lighting preset \"{this.Name}\" to \"{name}\" - Name is taken");
            return null;
        }
        string oldName = this.Name;
        this.Name = name;
        return oldName;
    }
}