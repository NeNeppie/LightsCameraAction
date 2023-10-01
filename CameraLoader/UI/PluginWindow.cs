using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

using CameraLoader.Config;

namespace CameraLoader.UI;

public partial class PluginWindow : Window
{
    private readonly CameraTab CameraTab;
    private readonly LightingTab LightingTab;

    public PluginWindow() : base("Lights, Camera, Action!")
    {
        this.IsOpen = false;
        this.Size = new Vector2(250f, 355f);
        this.SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(1500f, -1),
            MinimumSize = new Vector2(240f, -1)
        };
        this.SizeCondition = ImGuiCond.FirstUseEver;

        Service.GPoseHooking.OnGPoseStateChangeEvent += this.WindowBehaviourCheck;

        this.LightingTab = new LightingTab();
        this.CameraTab = new CameraTab();
    }

    public override void Draw()
    {
        if (!this.IsOpen) { return; }

        this.Flags = GetWindowFlags();

        if (ImGui.BeginTabBar("##TabBar", ImGuiTabBarFlags.None))
        {
            this.CameraTab.Draw();
            this.LightingTab.Draw();
            DrawSettingsTab();
#if DEBUG
            //DrawDebugTab();
#endif
            ImGui.EndTabBar();
        }

        var size = ImGui.GetWindowSize();
        ImGui.SetWindowSize(size with { Y = ImGui.GetCursorPosY() + (5f * ImGuiHelpers.GlobalScale) });
    }

    private static ImGuiWindowFlags GetWindowFlags()
    {
        var flags = ImGuiWindowFlags.NoScrollbar;
        if (Service.Config.LockWindowPosition)
            flags |= ImGuiWindowFlags.NoMove;
        if (Service.Config.LockWindowSize)
            flags |= ImGuiWindowFlags.NoResize;
        return flags;
    }

    private void WindowBehaviourCheck(bool entered)
    {
        if (Service.Config.WindowOpenMode == WindowOpenMode.OnEnterGPose)
        {
            this.IsOpen = entered;
        }
    }
}
