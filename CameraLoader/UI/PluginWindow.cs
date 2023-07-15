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
        Size = new Vector2(250f, 355f);
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(1500f, -1),
            MinimumSize = new Vector2(240f, -1)
        };
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
            DrawLightingTab();
            DrawSettingsTab();
#if DEBUG
            // Debug tab displaying window size
            ImGui.BeginTabItem(ImGui.GetWindowSize().ToString());
            ImGui.EndTabItem();
#endif
            ImGui.EndTabBar();
        }

        var size = ImGui.GetWindowSize();
        ImGui.SetWindowSize(size with { Y = ImGui.GetCursorPosY() + 5f });
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
