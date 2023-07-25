using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Event;

using CameraLoader.Game.Structs;

namespace CameraLoader.Game;

public unsafe class GameFunctions
{
    //[Siganure("E8 ?? ?? ?? ?? EB 28 83 FF 03")] EventGPoseController.LightRGB
    //[Signature("E8 ?? ?? ?? ?? EB 28 83 FF 03")] EventGPoseController.UnkSetLightSettings(nint, uint, float, float, float, int?)

    // LightObject.UpdateFalloffDistance(char unk)
    private delegate void UpdateFalloffDistanceDelegate(LightObject* obj, char unk);
    [Signature("48 89 5C 24 ?? 56 48 83 EC 50 48 8B D9")]  // ffxiv_dx11.exe+370670 | 140370670
    private readonly UpdateFalloffDistanceDelegate _updateFalloffDistance = null;

    // EventGPoseController.AddGPoseLight(nint, uint)
    private delegate char AddGPoseLightDelegate(EventGPoseController* EventGPoseController, uint LightIndex);
    [Signature("E8 ?? ?? ?? ?? 4C 39 26 0F 84")]  // ffxiv_dx11.exe+7EDA60 | 1407EDA60
    private readonly AddGPoseLightDelegate _addGPoseLight = null;

    // EventGPoseController.AddRemoveGPoseLight(nint, uint)
    private delegate char AddRemoveGPoseLightDelegate(EventGPoseController* EventGPoseController, uint LightIndex);
    [Signature("48 83 EC 28 4C 8B C1 83 FA 03")]
    private readonly AddRemoveGPoseLightDelegate _addRemoveGPoseLight = null;

    private delegate char UpdateUI(nint a1, nint a2);
    [Signature("E8 ?? ?? ?? ?? 84 C0 75 0C C6 86", DetourName = "UpdateUIDetour")]  // ffxiv_dx11.exe+C0C7D0 | 140C0C7D0
    private readonly Hook<UpdateUI> _updateUIHook = null;

    public GameFunctions()
    {
        SignatureHelper.Initialise(this);
        //this._updateUIHook.Enable();
    }

    public void UpdateFalloffDistance(LightObject* obj) => this._updateFalloffDistance!.Invoke(obj, '\0');

    public char AddGPoseLight(EventGPoseController* ptr, uint LightIndex) => _addGPoseLight!.Invoke(ptr, LightIndex);

    public char AddRemoveGPoseLight(EventGPoseController* ptr, uint LightIndex) => _addRemoveGPoseLight!.Invoke(ptr, LightIndex);

    public char UpdateUIDetour(nint a1, nint a2)
    {
        var result = _updateUIHook!.Original.Invoke(a1, a2);
        PluginLog.Debug($"UpdateUI(?) called: {a1:X}, {a2:X}. returns {(int)result}");
        return result;
    }

    public void Dispose()
    {
        //this._updateUIHook.Disable();
        //this._updateUIHook.Dispose();
    }
}