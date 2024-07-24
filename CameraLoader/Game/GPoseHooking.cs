using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

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
        var uiModule = Framework.Instance()->UIModule;
        var enterGPoseAddress = (nint)uiModule->VirtualTable->EnterGPose;
        var exitGPoseAddress = (nint)uiModule->VirtualTable->ExitGPose;

        this._enterGPoseHook = Service.GameInteropProvider.HookFromAddress<EnterGPoseDelegate>(enterGPoseAddress, this.EnterGPoseDetour);
        this._enterGPoseHook.Enable();

        this._exitGPoseHook = Service.GameInteropProvider.HookFromAddress<ExitGPoseDelegate>(exitGPoseAddress, this.ExitGPoseDetour);
        this._exitGPoseHook.Enable();
    }

    private bool EnterGPoseDetour(IntPtr addr)
    {
        var entered = this._enterGPoseHook!.Original.Invoke(addr);
        OnGPoseStateChangeEvent.Invoke(entered);
        return entered;
    }

    private void ExitGPoseDetour(IntPtr addr)
    {
        this._exitGPoseHook!.Original.Invoke(addr);
        OnGPoseStateChangeEvent.Invoke(false);
    }

    public void Dispose()
    {
        this._enterGPoseHook.Disable();
        this._exitGPoseHook.Disable();

        this._enterGPoseHook.Dispose();
        this._exitGPoseHook.Dispose();

        OnGPoseStateChangeEvent = null;
    }
}
