using System.Runtime.InteropServices;
using System.Numerics;

namespace CameraLoader.Game.Structs;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct DrawObject  // Client::Graphics::Scene::Object
{
    // [FieldOffset(0x00)] public nint VTable;
    [FieldOffset(0x18)] public DrawObject* ParentObject;
    [FieldOffset(0x20)] public DrawObject* PreviousSiblingObject;
    [FieldOffset(0x28)] public DrawObject* NextSiblingObject;
    [FieldOffset(0x30)] public DrawObject* ChildObject;
    [FieldOffset(0x50)] public Vector3 Position;
    [FieldOffset(0x60)] public Quaternion Rotation;
    [FieldOffset(0x70)] public Vector3 Scale;
    [FieldOffset(0x88)] public byte Flags;
    [FieldOffset(0x90)] public LightObject* LightObject;    // If GetObjectType() == 5
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct LightObject
{
    // [FieldOffset(0x00)] public nint VTable;
    [FieldOffset(0x18)] public bool isSourceVisible;        // Might not be a bool? Shows/Hides the "orb" (The object still emits light)
    [FieldOffset(0x20)] public nint InnerStruct;            // First 0x0C bytes are another position vector
    [FieldOffset(0x28)] public Vector3 RGB;
    [FieldOffset(0x34)] public float Intensity;             // (?) A value of 0 means the light is not visible
    // The distances at which lights simply cut out
    [FieldOffset(0x40)] public Vector3 MaxNegativeDistance; // Negative values. Gpose lights have "unlimited" (-10000) range
    [FieldOffset(0x50)] public Vector3 MaxPositiveDistance; // Gpose lights have "unlimited" (10000) range
    [FieldOffset(0x68)] public byte Type;                   // Type 1: 2, Type 2: 1, Type 3: 0 (See gpose lighting menu)
    [FieldOffset(0x8C)] public float FalloffDistance;       // Seems to be centered on the player?
}