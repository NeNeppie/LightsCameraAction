using System;
using System.Numerics;

using CameraLoader.Config;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow
{
    public static void DrawSettingsTab()
    {
        bool res = ImGui.BeginTabItem("Settings##SettingsTab");
        if (!res) { return; }

        ImGui.Spacing();

        ImGui.Text("Window Settings:");

        ImGui.Spacing();

        ImGui.Checkbox("Lock window position", ref Service.Config.LockWindowPosition);
        ImGui.Checkbox("Disable manual resizing", ref Service.Config.LockWindowSize);

        ImGui.Spacing();

        var selectedOpenMode = Service.Config.WindowOpenMode;
        ImGui.Text("Open Lights, Camera, Action...");
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.BeginCombo("##OpenLCAction", selectedOpenMode.GetDescription()))
        {
            foreach (var openMode in Enum.GetValues<WindowOpenMode>())
            {
                if (ImGui.Selectable(openMode.GetDescription(), openMode == selectedOpenMode))
                {
                    Service.Config.WindowOpenMode = openMode;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.CollapsingHeader("Height Settings"))
            DrawHeightSettings();

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
        ImGui.TextWrapped("\nHello there! This is my first plugin, so expect bugs to pop up here and there as I figure things out. " +
                            "Your feedback is greatly appreciated.");
        ImGui.Text($"\nHappy GPosing!");
        ImGui.PopStyleColor();

        ImGui.EndTabItem();
    }

    private static void DrawHeightSettings()
    {
        ImGui.TextWrapped("Preset selection box height:");
        ImGui.SliderInt("Camera", ref Service.Config.RowsVisibleCamera, 3, 10);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Number of rows shown when selecting a camera preset.");

        ImGui.SliderInt("Lighting", ref Service.Config.RowsVisibleLighting, 3, 10);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Number of rows shown when selecting a lighting preset.");

        ImGui.Spacing();
    }
}