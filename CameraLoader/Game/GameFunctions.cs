using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;

using CameraLoader.Game.Structs;

namespace CameraLoader.Game;

public unsafe class GameFunctions
{
    //[Signature("E8 ?? ?? ?? ?? 48 8B F0 48 89 B4 FB", DetourName = "LightDrawObjectCtor")]

    private delegate char CreateGPoseLightDelegate(nint EventFramework0x380, uint id);
    [Signature("E8 ?? ?? ?? ?? 4C 39 26 0F 84", DetourName = "CreateGPoseLightDetour")]
    private Hook<CreateGPoseLightDelegate> _createGPoseLightHook = null;

    private delegate char GPoseLightButtonDelegate(nint EventFramework0x380, uint id);
    [Signature("48 83 EC 28 4C 8B C1 83 FA 03", DetourName = "GPoseLightButtonDetour")]
    private Hook<GPoseLightButtonDelegate> _gPoseLightButtonHook = null;

    [Signature("48 83 EC 28 4C 8B C1 83 FA 03")]
    private readonly GPoseLightButtonDelegate _createDeleteGPoseLight = null;

    // LightObject.UpdateFalloffDistance(char unk)
    private delegate void UpdateFalloffDistanceDelegate(LightObject* obj, char unk);
    [Signature("48 89 5C 24 ?? 56 48 83 EC 50 48 8B D9")]  // ffxiv_dx11.exe+370670 | 140370670
    private readonly UpdateFalloffDistanceDelegate _updateFalloffDistance = null;

    public GameFunctions()
    {
        SignatureHelper.Initialise(this);
        //this._createGPoseLightHook.Enable();
        //this._gPoseLightButtonHook.Enable();
    }

    private char CreateGPoseLightDetour(nint EventGPoseController, uint id)
    {
        var ret = _createGPoseLightHook!.Original.Invoke(EventGPoseController, id);
        PluginLog.Debug($"CreateGPoseLight called with params EventFramework0x380: {EventGPoseController:X}, id: {id}. returns {(int)ret:X}");
        return ret;
    }

    private char GPoseLightButtonDetour(nint EventGPoseController, uint id)
    {
        var ret = _gPoseLightButtonHook!.Original.Invoke(EventGPoseController, id);
        PluginLog.Debug($"GPoseLightButton called with params EventFramework0x380: {EventGPoseController:X}, id: {id}. returns {(int)ret}");
        return ret;
    }

    // Does not update GPose's UI. Potential cause for crashes?
    private void CreateDeleteGPoseLight(nint EventGPoseController, uint id)
    {
        if (_createDeleteGPoseLight == null)
        {
            PluginLog.Debug("Error in CreateDeleteGPoseLight, idk");
            return;
        }
        var result = this._createDeleteGPoseLight!.Invoke(EventGPoseController, id);
        PluginLog.Debug(((int)result).ToString());
    }

    public void UpdateFalloffDistance(LightObject* obj)
    {
        this._updateFalloffDistance!.Invoke(obj, '\0');
    }

    public void Dispose()
    {
        //this._createGPoseLightHook.Disable();
        //this._gPoseLightButtonHook.Disable();

        //this._createGPoseLightHook.Dispose();
        //this._gPoseLightButtonHook.Dispose();
    }
}