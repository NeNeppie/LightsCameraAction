using System.Numerics;
using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow : Window
{
    public PluginWindow() : base("CameraLoader")
    {
        IsOpen = false;
        Size = new Vector2(250, 420);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        if (!IsOpen) { return; }

        if (ImGui.BeginTabBar("##TabBar", ImGuiTabBarFlags.None))
        {
            DrawCameraTab();
            ImGui.EndTabBar();
        }

    }
}
