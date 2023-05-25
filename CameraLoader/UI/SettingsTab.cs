using System;
using System.Numerics;

using CameraLoader.Config;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow
{
    public void DrawSettingsTab()
    {
        bool res = ImGui.BeginTabItem("Settings##SettingsTab");
        if (!res) { return; }

        // Drawing here
        ImGui.Text("Window Settings:");
        ImGui.Separator();
        ImGui.Checkbox("Lock window position", ref Service.Config.LockWindowPosition);
        ImGui.Checkbox("Disable manual resizing", ref Service.Config.LockWindowSize);

        var selectedOpenMode = Service.Config.WindowOpenMode;
        ImGui.Text("\nOpen CameraLoader...");
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.BeginCombo("##OpenCameraLoader", selectedOpenMode.GetDescription()))
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

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
        ImGui.TextWrapped("Hello there! This is my first plugin, so expect bugs to pop up here and there as I figure things out. " +
                            "Your help is greatly appreciated.");
        ImGui.Text($"\nHappy GPosing!");
        ImGui.PopStyleColor();

        ImGui.EndTabItem();
    }
}