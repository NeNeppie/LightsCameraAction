using System.Runtime.InteropServices;

namespace CameraLoader
{
    // Based off of https://github.com/Tenrys/ZoomTilt/tree/master/ZoomTilt/Structures , 
    // which is in turn based off of... a zoom hack. Yeah.
    // The camera's position, among other parameters, cannot be externally edited. The game simply resets it to that of the client
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GameCamera
    {
        [FieldOffset(0x114)] public float Distance;     // default is 6
        [FieldOffset(0x118)] public float MinDistance;  // 1.5
        [FieldOffset(0x11C)] public float MaxDistance;  // 20
        [FieldOffset(0x120)] public float FoV;          // default is 0.78
        [FieldOffset(0x124)] public float MinFoV;       // 0.69
        [FieldOffset(0x128)] public float MaxFoV;       // 0.78
        [FieldOffset(0x12C)] public float AddedFoV;     // -0.5 to 0.5, default is 0
        [FieldOffset(0x130)] public float HRotation;    // -pi -> pi, default is pi
        [FieldOffset(0x134)] public float VRotation;    // default is -0.349066 (?)
        [FieldOffset(0x148)] public float MinVRotation; // -1.483530
        [FieldOffset(0x14C)] public float MaxVRotation; // 0.785398 (pi/4)
        [FieldOffset(0x150)] public float Pan;          // -0.872664 to 0.872664, default is 0
        [FieldOffset(0x154)] public float Tilt;         // -0.646332 to 0.3417, affected by third person angle. default is 0
        [FieldOffset(0x160)] public float Roll;         // -pi -> pi, default is 0
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CameraManager
    {
        [FieldOffset(0x0)] public GameCamera* WorldCamera;
        [FieldOffset(0x8)] public GameCamera* IdleCamera;
        [FieldOffset(0x10)] public GameCamera* MenuCamera;
        [FieldOffset(0x18)] public GameCamera* SpectatorCamera;
    }
}