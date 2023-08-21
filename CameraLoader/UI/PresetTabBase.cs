using System.Numerics;

using CameraLoader.Game;

using ImGuiNET;

namespace CameraLoader.UI;

public abstract class PresetTabBase
{
    protected bool RenameOpen = false;
    protected string ErrorMessage = "";
    protected string SearchQuery = "";

    protected int SelectedMode = (int)PresetMode.Character;
    protected PresetBase SelectedPreset = null;
    protected int PresetIndex = -1;

    protected bool IsInGPose()
    {
        bool isInCameraMode = Service.Conditions[Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene];
        bool gposeActorExists = Service.ObjectTable[201] != null;
        if (!(isInCameraMode && gposeActorExists))
        {
            PresetIndex = -1;
            SelectedPreset = null;

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
        if (ErrorMessage != "")
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), ErrorMessage);
        }
    }
}