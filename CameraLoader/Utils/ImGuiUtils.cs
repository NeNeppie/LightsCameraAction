using System.Numerics;
using Dalamud.Interface;

using ImGuiNET;

namespace CameraLoader.Utils;

public class ImGuiButtonColors
{
    public Vector4 Normal;
    public Vector4 Hovered;
    public Vector4 Active;

    public ImGuiButtonColors(Vector4 normal, Vector4 hovered, Vector4 active)
    {
        Normal = normal;
        Hovered = hovered;
        Active = active;
    }
}

public static class ImGuiUtils
{
    public static ImGuiButtonColors Green = new ImGuiButtonColors(
        new Vector4(0.2f, 0.8f, 0.41f, 0.7f),
        new Vector4(0.2f, 0.9f, 0.41f, 0.7f),
        new Vector4(0.2f, 1f, 0.41f, 0.7f));

    public static ImGuiButtonColors Red = new ImGuiButtonColors(
        new Vector4(0.78f, 0.33f, 0.33f, 0.7f),
        new Vector4(0.88f, 0.33f, 0.33f, 0.7f),
        new Vector4(0.99f, 0.33f, 0.33f, 0.7f));

    public static ImGuiButtonColors Blue = new ImGuiButtonColors(
        new Vector4(0.2f, 0.5f, 0.9f, 0.7f),
        new Vector4(0.2f, 0.6f, 1f, 0.7f),
        new Vector4(0.25f, 0.65f, 1f, 0.7f));

    public static ImGuiButtonColors Yellow = new ImGuiButtonColors(
        new Vector4(0.8f, 0.82f, 0.2f, 0.7f),
        new Vector4(0.9f, 0.92f, 0.3f, 0.7f),
        new Vector4(1f, 1f, 0.4f, 0.7f));

    public static ImGuiButtonColors Orange = new ImGuiButtonColors(
        new Vector4(0.8f, 0.65f, 0.33f, 0.7f),
        new Vector4(0.9f, 0.75f, 0.33f, 0.7f),
        new Vector4(1f, 0.85f, 0.33f, 0.7f));

    public static bool ColoredButton(string label, ImGuiButtonColors colors, Vector2 size = default, string tooltip = null, bool small = false)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, colors.Normal);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, colors.Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, colors.Active);

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

    public static bool ColoredIconButton(FontAwesomeIcon icon, ImGuiButtonColors colors, Vector2 size = default, string tooltip = null, bool small = false)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, colors.Normal);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, colors.Hovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, colors.Active);

        bool res = IconButton(icon, size, tooltip, small);

        ImGui.PopStyleColor(3);
        return res;
    }

    public static void IconText(FontAwesomeIcon icon)
    {
        var text = icon.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(text);
        ImGui.PopFont();
    }
}