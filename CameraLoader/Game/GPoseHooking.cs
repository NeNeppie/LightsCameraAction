using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace CameraLoader.Game;

public class GPoseHooking : IDisposable
{
    public delegate void OnEnterGposeDelegate();
    public event OnEnterGposeDelegate OnEnterGposeEvent;

    public delegate bool EnterGPoseDelegate(IntPtr addr);
    private Hook<EnterGPoseDelegate> _enterGPoseDelegateHook = null;

    public unsafe GPoseHooking()
    {
        UIModule* uiModule = Framework.Instance()->GetUiModule();
        var enterGPoseAddress = (nint)uiModule->VTable->EnterGPose;
        _enterGPoseDelegateHook = Hook<EnterGPoseDelegate>.FromAddress(enterGPoseAddress, this.EnterGPoseDetour);
        _enterGPoseDelegateHook.Enable();
    }

    private bool EnterGPoseDetour(IntPtr addr)
    {
        var entered = _enterGPoseDelegateHook!.Original.Invoke(addr);
        if (entered)
        {
            OnEnterGposeEvent.Invoke();
        }
        return entered;
    }

    public void Dispose()
    {
        this._enterGPoseDelegateHook.Disable();
        this._enterGPoseDelegateHook.Dispose();
        OnEnterGposeEvent = null;
    }
}