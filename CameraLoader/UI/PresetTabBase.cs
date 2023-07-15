using System.Numerics;

using CameraLoader.Game;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow
{
    private int _selected = -1;
    private bool _renameOpen = false;
    private string _errorMessage = "";
    private string _searchQuery = "";
    private static int _presetMode = (int)PresetMode.Character;

    private bool IsInGPose()
    {
        bool isInCameraMode = Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene];
        bool gposeActorExists = Service.ObjectTable[201] != null;
        if (!(isInCameraMode && gposeActorExists))
        {
            this._selectedPreset = null;
            this._selectedPresetL = null;
            this._selected = -1;

            ImGui.TextWrapped("Unavailable outside of Group Pose");
            ImGui.Separator();
            return false;
        }
        return true;
    }

    private void DrawPresetModeSelection()
    {
        ImGui.Text("Preset Mode:");
        ImGui.BeginGroup();
        {
            ImGui.RadioButton("Character Position", ref _presetMode, (int)PresetMode.Character);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Presets are saved and loaded relative to your character's orientation");
            }
            ImGui.RadioButton("Camera Position", ref _presetMode, (int)PresetMode.Camera);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Presets are saved relative to the camera's current orientation.\nYour character's orientation is not taken into account");
            }
        }
        ImGui.EndGroup();
    }

    private void DrawErrorMessage()
    {
        if (_errorMessage != "")
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), _errorMessage);
        }
    }
}