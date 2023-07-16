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

        // Drawing here
        bool isInGPose = this.IsInGPose();
        if (!isInGPose)
        {
            _cameraIndex = -1;
            _cameraPreset = null;

            ImGui.TextWrapped("Unavailable outside of Group Pose");
            ImGui.Separator();
            ImGui.BeginDisabled();
        }

        ImGuiUtils.IconText(FontAwesomeIcon.CameraRetro); ImGui.SameLine();
        this.DrawPresetModeSelection();

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 60f);
        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Green, size: new Vector2(60f, 60f), tooltip: "Create a new preset"))
        {
            var preset = new CameraPreset(_presetMode);
            Service.Config.CameraPresetNames.Add(preset.Name);
            Service.Config.CameraPresets.Add(preset);
            Service.Config.Save();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
        // TODO: Adjustable height based on rows
        ImGui.BeginChild("Preset Menu##Camera", new Vector2(0.0f, 300f), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref _searchQuery, 30);
        ImGui.PopItemWidth();

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

        if (_cameraPreset != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            ImGui.BeginChild("Preset Detail", new Vector2(0.0f, 230f), true);
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

                    if (oldName != null)
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
        string foVFormatted = preset.GposeFoV > 0 ? $"({preset.ZoomFoV:F2}+{preset.GposeFoV})" : $"({preset.ZoomFoV:F2}{preset.GposeFoV})";

        ImGui.TextWrapped(preset.Name);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));

        ImGui.TextWrapped($"Mode: {(PresetMode)preset.PositionMode} Position");
        ImGui.Text($"Zoom: {preset.Distance:F2} , FoV: {(preset.ZoomFoV + preset.GposeFoV):F2} " + foVFormatted);
        ImGui.Text($"H: {MathUtils.RadToDeg(preset.HRotation):F2}\x00B0 , V: {MathUtils.RadToDeg(preset.VRotation):F2}\x00B0");

        ImGui.Text($"Pan: {MathUtils.RadToDeg(preset.Pan):F0}\x00B0 , "); ImGui.SameLine();
        ImGui.Text($"Tilt: {MathUtils.RadToDeg(preset.Tilt):F0}\x00B0 , "); ImGui.SameLine();
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