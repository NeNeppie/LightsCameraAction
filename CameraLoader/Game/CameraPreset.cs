using CameraLoader.Utils;

namespace CameraLoader.Game;

public unsafe class CameraPreset : PresetBase
{
    public float Distance { get; set; }
    public float HRotation { get; set; }
    public float VRotation { get; set; }
    public float ZoomFoV { get; set; }  // Applies when zooming in very closely
    public float GposeFoV { get; set; } // Can be adjusted in the GPose settings menu
    public float Pan { get; set; }
    public float Tilt { get; set; }
    public float Roll { get; set; }

    public CameraPreset() { }
    public CameraPreset(string name, int mode = 0)
    {
        float cameraRot = _camera->HRotation;
        float relativeRot = cameraRot;

        if (mode == (int)PresetMode.CharacterOrientation)
        {
            var playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
            relativeRot = MathUtils.ConvertToRelative(cameraRot, playerRot);
        }

        // First Person Mode
        if (_camera->Mode == 0) { relativeRot = MathUtils.SubPiRad(relativeRot); }

        this.Name = name;
        this.PositionMode = mode;
        this.Distance = (_camera->Mode == 0) ? 0f : _camera->Distance;
        this.HRotation = relativeRot;
        this.VRotation = _camera->VRotation;
        this.ZoomFoV = _camera->FoV;
        this.GposeFoV = _camera->AddedFoV;
        this.Pan = _camera->Pan;
        this.Tilt = _camera->Tilt;
        this.Roll = _camera->Roll;
    }

    public bool IsValid()
    {
        // Doesn't go above Max, but Max can be externally modified
        if (this.Distance > 20f)
            return false;

        // Zoom FoV carries outside of gpose! Negative values flip the screen, High positive values are effectively a zoom hack
        // Gpose FoV resets when exiting gpose, but we don't want people suddenly entering gpose during a fight.
        if (this.ZoomFoV < 0.69f || this.ZoomFoV > 0.78f || this.GposeFoV < -0.5f || this.GposeFoV > 0.5f)
            return false;

        // Both reset when exiting gpose, but can still be modified beyond the limits the game sets
        if (this.Pan < -0.873f || this.Pan > 0.873f || this.Tilt < -0.647f || this.Tilt > 0.342f)
            return false;

        return true;
    }

    public override bool Load()
    {
        if (!this.IsValid()) { return false; }

        float hRotation = this.HRotation;
        if (this.PositionMode == (int)PresetMode.CharacterOrientation)
        {
            var playerRot = Service.ClientState.LocalPlayer?.Rotation ?? 0f;
            hRotation = MathUtils.ConvertFromRelative(this.HRotation, playerRot);
        }

        // First Person Mode
        if (_camera->Mode == 0) { hRotation = MathUtils.AddPiRad(hRotation); }

        _camera->Mode = (this.Distance == 0) ? 0 : 1;
        _camera->Distance = (this.Distance == 0) ? 1.5f : this.Distance;
        _camera->HRotation = hRotation;
        _camera->VRotation = this.VRotation;
        _camera->FoV = this.ZoomFoV;
        _camera->AddedFoV = this.GposeFoV;
        _camera->Pan = this.Pan;
        _camera->Tilt = this.Tilt;
        _camera->Roll = this.Roll;

        return true;
    }
}
