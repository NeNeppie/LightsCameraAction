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

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
        ImGui.TextWrapped("\nHello there! This is my first plugin, so expect bugs to pop up here and there as I figure things out. " +
                            "Your feedback is greatly appreciated.");
        ImGui.Text($"\nHappy GPosing!");
        ImGui.PopStyleColor();

        ImGui.EndTabItem();
    }
}