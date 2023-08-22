using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace CameraLoader.Game;

public class GPoseHooking : IDisposable
{
    public delegate void OnGPoseStateChangeDelegate(bool entered);
    public event OnGPoseStateChangeDelegate OnGPoseStateChangeEvent;

    public delegate bool EnterGPoseDelegate(IntPtr addr);
    private Hook<EnterGPoseDelegate> _enterGPoseHook = null;

    public delegate void ExitGPoseDelegate(IntPtr addr);
    private Hook<ExitGPoseDelegate> _exitGPoseHook = null;

    public unsafe GPoseHooking()
    {
        UIModule* uiModule = Framework.Instance()->GetUiModule();
        var enterGPoseAddress = (nint)uiModule->VTable->EnterGPose;
        var exitGPoseAddress = (nint)uiModule->VTable->ExitGPose;

        _enterGPoseHook = Hook<EnterGPoseDelegate>.FromAddress(enterGPoseAddress, this.EnterGPoseDetour);
        _exitGPoseHook = Hook<ExitGPoseDelegate>.FromAddress(exitGPoseAddress, this.ExitGPoseDetour);

        _enterGPoseHook.Enable();
        _exitGPoseHook.Enable();
    }

    private bool EnterGPoseDetour(IntPtr addr)
    {
        var entered = _enterGPoseHook!.Original.Invoke(addr);
        if (entered)
        {
            OnGPoseStateChangeEvent.Invoke(true);
        }
        return entered;
    }

    private void ExitGPoseDetour(IntPtr addr)
    {
        _exitGPoseHook!.Original.Invoke(addr);
        OnGPoseStateChangeEvent.Invoke(false);
    }

    public void Dispose()
    {
        _enterGPoseHook.Disable();
        _exitGPoseHook.Disable();

        _enterGPoseHook.Dispose();
        _exitGPoseHook.Dispose();

        OnGPoseStateChangeEvent = null;
    }
}