using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace CameraLoader.Utils;

public static class ImGuiUtils
{
    public static Vector4 Green => new(0.2f, 0.8f, 0.41f, 0.7f);
    public static Vector4 Red => new(0.78f, 0.33f, 0.33f, 0.7f);
    public static Vector4 Blue => new(0.2f, 0.5f, 0.9f, 0.7f);
    public static Vector4 Yellow => new(0.87f, 0.87f, 0.3f, 0.7f);
    public static Vector4 Orange => new(0.9f, 0.65f, 0.2f, 0.7f);

    public static Vector2 ButtonSizeLarge => new Vector2(2.5f * ImGui.GetFontSize()) + ImGui.GetStyle().FramePadding;
    public static Vector2 ButtonSize => new Vector2(1.5f * ImGui.GetFontSize()) + ImGui.GetStyle().FramePadding;
    public static float FrameRounding => ImGuiHelpers.GlobalScale * 4f;

    public static bool ColoredButton(string label, Vector4 color, Vector2 size = default, string tooltip = null, bool small = false)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, color);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color * new Vector4(1f, 1f, 1f, 1.2f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, color * new Vector4(1f, 1f, 1f, 1.5f));

        bool res = small ? ImGui.SmallButton(label) : ImGui.Button(label, size);
        if (tooltip != null && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        // Tooltip is unaffected
        ImGui.PopStyleColor(3);
        return res;
    }

    public static bool IconButton(FontAwesomeIcon icon, Vector2 size = default, string tooltip = null, bool small = false)
    {
        var label = icon.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        bool res = small ? ImGui.SmallButton(label) : ImGui.Button(label, size);
        ImGui.PopFont();

        if (tooltip != null && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }

        return res;
    }

    public static bool ColoredIconButton(FontAwesomeIcon icon, Vector4 color, Vector2 size = default, string tooltip = null, bool small = false)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, color);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color * new Vector4(1f, 1f, 1f, 1.2f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, color * new Vector4(1f, 1f, 1f, 1.5f));

        bool res = IconButton(icon, size, tooltip, small);

        ImGui.PopStyleColor(3);
        return res;
    }

    public static void IconText(FontAwesomeIcon icon, Vector4? color = null, string tooltip = null)
    {
        var text = icon.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        if (color != null)
        {
            ImGui.TextColored(color.Value, text);
        }
        else
        {
            ImGui.Text(text);
        }
        ImGui.PopFont();

        if (tooltip != null && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip);
        }
    }
}