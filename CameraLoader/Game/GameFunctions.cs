using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Event;

using CameraLoader.Game.Structs;

namespace CameraLoader.Game;

public unsafe class GameFunctions
{
    // EventGPoseController.ToggleGPoseLight(uint index)
    private delegate char ToggleLightDelegate(EventGPoseController* EventGPoseController, uint index);
    [Signature("48 83 EC 28 4C 8B C1 83 FA 03")]
    private readonly ToggleLightDelegate _toggleLight = null!;

    // EventGpPoseController.SetLightType(uint index, Vector3 RGB, int type)
    // private delegate void SetLightTypeDelegate(EventGPoseController* EventGPoseController, uint index, Vector3 RGB, int type);
    // [Signature("83 FA 03 0F 83 ?? ?? ?? ?? F3 0F 11 5C 24 ??")]
    // private readonly SetLightTypeDelegate _setLightType = null;

    // LightObject.UpdateMaterials()
    private delegate void LightUpdateMaterialsDelegate(LightObject* obj);
    [Signature("40 53 48 83 EC 20 0F B6 81 ?? ?? ?? ?? 48 8B D9 A8 04 75 45 0C 04 B2 05")]
    private readonly LightUpdateMaterialsDelegate _lightUpdateMaterials = null!;

    // LightObject.UpdateCulling()
    private delegate void LightUpdateCullingDelegate(LightObject* obj);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 40 48 8B B9 ?? ?? ?? ??")]
    private readonly LightUpdateCullingDelegate _lightUpdateCulling = null!;

    // LightRenderObject.UpdateTypeRange(char unk) "48 89 5C 24 ?? 56 48 83 EC 50 48 8B D9"
    private delegate void LightUpdateRangeDelegate(LightRenderObject* obj, char unk);
    [Signature("E8 ?? ?? ?? ?? 48 8D 8F ?? ?? ?? ?? FF 15 ?? ?? ?? ?? 48 8D 8F ?? ?? ?? ??")]
    private readonly LightUpdateRangeDelegate _lightUpdateRange = null!;

    public GameFunctions()
    {
        Service.GameInteropProvider.InitializeFromAttributes(this);
    }

    public char ToggleLight(EventGPoseController* ptr, uint index) => this._toggleLight.Invoke(ptr, index);

    // public void SetLightType(EventGPoseController* ptr, uint index, Vector3 RGB, int type) => this._setLightType.Invoke(ptr, index, RGB, type);
    
    public void UpdateLightObject(LightObject* obj)
    {
        if (obj != null)
        {
            this._lightUpdateRange.Invoke(obj->LightRenderObject, '\0');
            // Credit to Ktisis for the signatures of these two.
            this._lightUpdateCulling.Invoke(obj);
            this._lightUpdateMaterials.Invoke(obj);
        }
    }

    public void Dispose()
    {
    }
}
