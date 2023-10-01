using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Event;

using CameraLoader.Game.Structs;

namespace CameraLoader.Game;

public unsafe class GameFunctions
{
    //[Siganure("E8 ?? ?? ?? ?? EB 28 83 FF 03")] EventGPoseController.LightRGB
    //[Signature("E8 ?? ?? ?? ?? EB 28 83 FF 03")] EventGPoseController.UnkSetLightSettings(nint, uint, float, float, float, int?)
    //[Signature("E8 ?? ?? ?? ?? 84 C0 75 0C C6 86")] Unknown Update UI function (specifically for CameraSetting agent?)

    // LightObject.UpdateFalloffDistance(char unk)
    private delegate void UpdateFalloffDistanceDelegate(LightObject* obj, char unk);
    [Signature("48 89 5C 24 ?? 56 48 83 EC 50 48 8B D9")]  // ffxiv_dx11.exe+370670 | 140370670
    private readonly UpdateFalloffDistanceDelegate _updateFalloffDistance = null;

    // EventGPoseController.AddGPoseLight(nint, uint)
    private delegate char AddGPoseLightDelegate(EventGPoseController* EventGPoseController, uint LightIndex);
    [Signature("E8 ?? ?? ?? ?? 4C 39 26 0F 84")]  // ffxiv_dx11.exe+7EDA60 | 1407EDA60
    private readonly AddGPoseLightDelegate _addGPoseLight = null;

    // EventGPoseController.ToggleGPoseLight(nint, uint)
    private delegate char ToggleGPoseLightDelegate(EventGPoseController* EventGPoseController, uint LightIndex);
    [Signature("48 83 EC 28 4C 8B C1 83 FA 03")]
    private readonly ToggleGPoseLightDelegate _toggleGPoseLight = null;

    public GameFunctions()
    {
        Service.GameInteropProvider.InitializeFromAttributes(this);
    }

    public void UpdateFalloffDistance(LightObject* obj) => this._updateFalloffDistance!.Invoke(obj, '\0');

    public char AddGPoseLight(EventGPoseController* ptr, uint LightIndex) => this._addGPoseLight!.Invoke(ptr, LightIndex);

    public char ToggleGPoseLight(EventGPoseController* ptr, uint LightIndex) => this._toggleGPoseLight!.Invoke(ptr, LightIndex);

    public void Dispose()
    {
    }
}
