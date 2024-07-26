using System.Runtime.InteropServices;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.Graphics;

namespace CameraLoader.Game.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0xA0)]
public unsafe struct LightObject
{
    [FieldOffset(0x00)] public unsafe nint* VTable;
    [FieldOffset(0x00)] public DrawObject DrawObject; // Client::Graphics::Scene::Object
    [FieldOffset(0x50)] public Vector3 Position;
    [FieldOffset(0x60)] public Quaternion Rotation;
    [FieldOffset(0x70)] public Vector3 Scale;
    [FieldOffset(0x88)] public byte Flags;
    [FieldOffset(0x90)] public LightRenderObject* LightRenderObject; // If GetObjectType() == 5
}

[StructLayout(LayoutKind.Explicit, Size = 0xA0)]
public unsafe struct LightRenderObject
{
    [FieldOffset(0x00)] public nint* VTable;
    [FieldOffset(0x18)] public uint LightFlags;             // Light emission flags
    [FieldOffset(0x1C)] public uint EmissionType;           // Irrelevant for Gpose lights
    [FieldOffset(0x20)] public Transform* Transform;
    [FieldOffset(0x28)] public Vector3 RGB;
    [FieldOffset(0x34)] public float Intensity;
    [FieldOffset(0x40)] public Vector3 MaxRangeNegative;    // Negative values. Gpose lights have "unlimited" (-10000) range
    [FieldOffset(0x50)] public Vector3 MaxRangePositive;    // Gpose lights have "unlimited" (10000) range
    [FieldOffset(0x68)] public byte Type;                   // Type 1: 2 (Cubic), Type 2: 1 (Quadratic), Type 3: 0 (Linear)
    [FieldOffset(0x8C)] public float Range;                 // Seems to be centered on the player?
}
