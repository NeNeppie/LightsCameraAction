using System.Numerics;
using Dalamud.Interface.Windowing;

using CameraLoader.Config;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow : Window
{
    public PluginWindow() : base("CameraLoader")
    {
        IsOpen = false;
        Size = new Vector2(250, 420);
        SizeCondition = ImGuiCond.FirstUseEver;

        Service.GPoseHooking.OnEnterGposeEvent += WindowBehaviourCheck;
    }

    public override void Draw()
    {
        if (!IsOpen) { return; }

        this.Flags = GetWindowFlags();

        if (ImGui.BeginTabBar("##TabBar", ImGuiTabBarFlags.None))
        {
            DrawCameraTab();
            DrawOptionsTab();
            ImGui.EndTabBar();
        }
    }

    private ImGuiWindowFlags GetWindowFlags()
    {
        var flags = ImGuiWindowFlags.None;
        if (Service.Config.LockWindowPosition) { flags |= ImGuiWindowFlags.NoMove; }
        if (Service.Config.LockWindowSize) { flags |= ImGuiWindowFlags.NoResize; }
        return flags;
    }

    private void WindowBehaviourCheck()
    {
        if (Service.Config.WindowOpenMode == WindowOpenMode.OnEnterGPose)
        {
            this.IsOpen = true;
        }
    }
}
