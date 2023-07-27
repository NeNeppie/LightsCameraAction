using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

using CameraLoader.Game;
using CameraLoader.Utils;

using ImGuiNET;

namespace CameraLoader.UI;

public partial class PluginWindow
{
    private int _lightingIndex = -1;
    private LightingPreset _lightingPreset = null;

    public void DrawLightingTab()
    {
        bool res = ImGui.BeginTabItem("Lighting##LightingTab");
        if (!res) { return; }

        // Drawing here
        bool isInGPose = this.IsInGPose();
        if (!isInGPose)
        {
            _lightingIndex = -1;
            _lightingPreset = null;

            ImGui.TextWrapped("Unavailable outside of Group Pose");
            ImGui.Separator();
            ImGui.BeginDisabled();
        }

        ImGuiUtils.IconText(FontAwesomeIcon.Lightbulb); ImGui.SameLine();
        this.DrawPresetModeSelection();

        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 60f);
        if (ImGuiUtils.ColoredIconButton(FontAwesomeIcon.Plus, ImGuiUtils.Yellow, size: new Vector2(60f, 60f), tooltip: "Create a new preset"))
        {
            var preset = new LightingPreset(_presetMode);
            Service.Config.LightingPresetNames.Add(preset.Name);
            Service.Config.LightingPresets.Add(preset);
            Service.Config.Save();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
        // TODO: Adjustable height based on rows
        ImGui.BeginChild("Preset Menu##Lighting", new Vector2(0.0f, 300f), true);
        ImGui.PopStyleVar(1);

        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "Search...", ref _searchQuery, 30);
        ImGui.PopItemWidth();

        for (int i = 0; i < Service.Config.LightingPresets.Count; i++)
        {
            var preset = Service.Config.LightingPresets[i];
            if (!preset.Name.ToLower().Contains(_searchQuery.ToLower())) { continue; }

            bool isCurrentSelected = _lightingIndex == i;
            if (ImGui.Selectable($"{preset.Name}##Lighting", isCurrentSelected))
            {
                this._lightingIndex = isCurrentSelected ? -1 : i;
                this._lightingPreset = isCurrentSelected ? null : preset;

                this._renameOpen = false;
                this._errorMessage = "";
            }
        }
        ImGui.EndChild();

        if (_lightingPreset != null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            ImGui.BeginChild("Preset Detail", new Vector2(0.0f, 415f), true);
            ImGui.PopStyleVar(1);

            DrawPresetInfo(ref _lightingPreset);
            if (ImGuiUtils.ColoredButton("Load Preset", ImGuiUtils.Blue))
            {
                _lightingPreset.Load();
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
                RemovePreset(ref _lightingPreset);
                this._lightingIndex = -1;
                this._lightingPreset = null;
            }

            if (_renameOpen)
            {
                string newName = _lightingPreset.Name;
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.InputText("##RenameLighting", ref newName, 30, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    var oldName = _lightingPreset.Rename(newName);
                    this._renameOpen = false;

                    if (oldName != null)
                    {
                        Service.Config.LightingPresetNames.Remove(oldName);
                        Service.Config.LightingPresetNames.Add(newName);
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

    private void DrawPresetInfo(ref LightingPreset preset)
    {
        ImGui.TextWrapped(preset.Name);

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1f));
        ImGui.TextWrapped($"Mode: {(PresetMode)preset.PositionMode} Position");

        int i = 1;
        foreach (var light in preset.Lights)
        {
            ImGui.Separator();
            if (!light.Active) { ImGui.BeginDisabled(); }

            ImGui.Text($"Light {i++} | Type: {3 - light.Type}");
            ImGui.Text($"Position: {light.relativePos.ToString("F2")}");

            if (preset.PositionMode == (int)PresetMode.Character)
            {
                ImGui.SameLine();
                ImGui.Text($"({MathUtils.RadToDeg(light.relativeRot):F2}\x00B0)");
            }

            var color = light.RGB != Vector3.Zero ? MathUtils.ConvertFloatsTo24BitColor(light.RGB) : Vector3.Zero;
            ImGuiUtils.IconText(FontAwesomeIcon.Circle, ImGuiColors.DPSRed); ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DPSRed, $"R: {color.X:F0}, "); ImGui.SameLine();
            ImGuiUtils.IconText(FontAwesomeIcon.Circle, ImGuiColors.HealerGreen); ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.HealerGreen, $"G: {color.Y:F0}, "); ImGui.SameLine();
            ImGuiUtils.IconText(FontAwesomeIcon.Circle, ImGuiColors.TankBlue); ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.TankBlue, $"B: {color.Z:F0}");

            if (!light.Active) { ImGui.EndDisabled(); }
        }
        ImGui.PopStyleColor(1);
    }

    private void RemovePreset(ref LightingPreset preset)
    {
        Service.Config.LightingPresetNames.Remove(preset.Name);
        Service.Config.LightingPresets.Remove(preset);
        Service.Config.Save();
    }
}