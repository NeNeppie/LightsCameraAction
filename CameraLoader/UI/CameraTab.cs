using System.Numerics;
using Dalamud.Interface;
using Dalamud.Logging;

using CameraLoader.Game;
using CameraLoader.Utils;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow
{
    private int _cameraIndex = -1;
    private CameraPreset _cameraPreset = null;

    public void DrawCameraTab()
    {
        bool res = ImGui.BeginTabItem("Camera##CameraTab");
        if (!res) { return; }

        ImGui.Spacing();

        bool isInGPose = this.IsInGPose();
        if (!isInGPose)
        {
            _cameraIndex = -1;
            _cameraPreset = null;

            ImGui.TextWrapped("Unavailable outside of Group Pose");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginDisabled();
        }

        // Preset saving
        ImGuiUtils.IconText(FontAwesomeIcon.CameraRetro);
        ImGui.SameLine();

        this.DrawPresetModeSelection();
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 40f * ImGuiHelpers.GlobalScale);

        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Green, size: new Vector2(40f, 40f) * ImGuiHelpers.GlobalScale, tooltip: "Create a new preset"))
        {
            var preset = new CameraPreset(_presetMode);
            Service.Config.CameraPresetNames.Add(preset.Name);
            Service.Config.CameraPresets.Add(preset);
            Service.Config.Save();
        }

        ImGui.Spacing();

        // Preset selection
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 4f * ImGuiHelpers.GlobalScale);
        // TODO: Adjustable height based on rows
        ImGui.BeginChild("Preset Menu##Camera", new Vector2(0f, 200f * ImGuiHelpers.GlobalScale), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref _searchQuery, 30);
        ImGui.PopItemWidth();

        ImGui.Spacing();

        for (int i = 0; i < Service.Config.CameraPresets.Count; i++)
        {
            var preset = Service.Config.CameraPresets[i];
            if (!preset.Name.ToLower().Contains(_searchQuery.ToLower())) { continue; }

            bool isCurrentSelected = _cameraIndex == i;
            if (ImGui.Selectable($"{preset.Name}##Camera", isCurrentSelected))
            {
                this._cameraIndex = isCurrentSelected ? -1 : i;
                this._cameraPreset = isCurrentSelected ? null : preset;

                this._renameOpen = false;
                this._errorMessage = "";
            }
        }
        ImGui.EndChild();

        // Preset information
        if (_cameraPreset is not null)
        {
            ImGui.Spacing();

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 4f * ImGuiHelpers.GlobalScale);
            ImGui.BeginChild("Preset Detail", new Vector2(0f, 155f * ImGuiHelpers.GlobalScale), true);
            ImGui.PopStyleVar(1);

            DrawPresetInfo(ref _cameraPreset);

            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                if (!_cameraPreset.Load())
                {
                    PluginLog.Information($"Attempted to load invalid Camera Preset \"{_cameraPreset.Name}\"");
                    this._errorMessage = "Preset is invalid";
                }
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Rename", ImGuiUtils.Orange))
            {
                this._renameOpen = !_renameOpen;
                this._errorMessage = "";
            }
            ImGui.SameLine();

            if (ImGuiUtils.ColoredButton("Remove", ImGuiUtils.Red))
            {
                RemovePreset(ref _cameraPreset);
                this._cameraIndex = -1;
                this._cameraPreset = null;
            }

            if (_renameOpen)
            {
                string newName = _cameraPreset.Name;
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.InputText("##RenameCamera", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    var oldName = _cameraPreset.Rename(newName);
                    this._renameOpen = false;

                    if (oldName is not null)
                    {
                        Service.Config.CameraPresetNames.Remove(oldName);
                        Service.Config.CameraPresetNames.Add(newName);
                        Service.Config.Save();
                    }
                    else
                    {
                        this._errorMessage = "Names must be unique";
                    }
                }
                ImGui.PopItemWidth();
            }

            this.DrawErrorMessage();
            ImGui.EndChild();
        }

        if (!isInGPose) { ImGui.EndDisabled(); }
        ImGui.EndTabItem();
    }

    private void DrawPresetInfo(ref CameraPreset preset)
    {
        // FIXME: formatting bug when GposeFoV is 0
        string fov = preset.GposeFoV > 0 ? $"({preset.ZoomFoV:F2}+{preset.GposeFoV})" : $"({preset.ZoomFoV:F2}{preset.GposeFoV:F2})";

        ImGui.TextWrapped(preset.Name);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));

        ImGui.TextWrapped($"Mode: {(PresetMode)preset.PositionMode} Position");
        ImGui.Text($"Zoom: {preset.Distance:F2} , FoV: {(preset.ZoomFoV + preset.GposeFoV):F2} " + fov);
        ImGui.Text($"H: {MathUtils.RadToDeg(preset.HRotation):F2}\x00B0 , V: {MathUtils.RadToDeg(preset.VRotation):F2}\x00B0");

        ImGui.Text($"Pan: {MathUtils.RadToDeg(preset.Pan):F0}\x00B0 , ");
        ImGui.SameLine();

        ImGui.Text($"Tilt: {MathUtils.RadToDeg(preset.Tilt):F0}\x00B0 , ");
        ImGui.SameLine();

        ImGui.Text($"Roll: {MathUtils.RadToDeg(preset.Roll):F0}\x00B0");

        ImGui.PopStyleColor(1);
    }

    private void RemovePreset(ref CameraPreset preset)
    {
        Service.Config.CameraPresetNames.Remove(preset.Name);
        Service.Config.CameraPresets.Remove(preset);
        Service.Config.Save();
    }
}