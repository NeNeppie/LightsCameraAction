using System.Numerics;
using ImGuiNET;

using CameraLoader.Game;

namespace CameraLoader.UI;

public abstract class PresetTabBase
{
    protected bool RenameOpen = false;
    protected string ErrorMessage = "";
    protected string SearchQuery = "";

    protected int SelectedMode = (int)PresetMode.CharacterOrientation;
    protected PresetBase SelectedPreset = null;
    protected int PresetIndex = -1;

    protected bool IsInGPose()
    {
        var isInCameraMode = Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene];
        var gposeActorExists = Service.ObjectTable[201] != null;
        if (!(isInCameraMode && gposeActorExists))
        {
            this.PresetIndex = -1;
            this.SelectedPreset = null;

            ImGui.TextWrapped("Unavailable outside of Group Pose");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginDisabled();
            return false;
        }
        return true;
    }

    protected void DrawErrorMessage()
    {
        if (this.ErrorMessage != "")
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), ErrorMessage);
        }
    }
}
