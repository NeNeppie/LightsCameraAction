using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

using CameraLoader.Config;
using CameraLoader.Game;

namespace CameraLoader.UI;

public partial class PluginWindow : Window
{
    private readonly PresetTab _cameraTab;
    private readonly PresetTab _lightingTab;

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

        this._cameraTab = new PresetTab(typeof(CameraPreset));
        this._lightingTab = new PresetTab(typeof(LightingPreset));
    }

    public override void Draw()
    {
        if (!this.IsOpen) { return; }

        this.Flags = GetWindowFlags();

        if (ImGui.BeginTabBar("##TabBar", ImGuiTabBarFlags.None))
        {
            this._cameraTab.Draw(ImGui.GetFrameHeightWithSpacing() + (ImGui.GetTextLineHeightWithSpacing() * Service.Config.RowsVisibleCamera),
                (ImGui.GetTextLineHeightWithSpacing() * 5f) + (ImGui.GetFrameHeightWithSpacing() * 2f));
            this._lightingTab.Draw(ImGui.GetFrameHeightWithSpacing() + (ImGui.GetTextLineHeightWithSpacing() * Service.Config.RowsVisibleLighting),
                (ImGui.GetTextLineHeightWithSpacing() * 8f) + ((ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y) * 3f));
            DrawSettingsTab();
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
