using System;

namespace CameraLoader.Utils;

public static class MathUtils
{
    public static float RadToDeg(float radians)
    {
        return (float)(radians * (180 / Math.PI));
    }

    public static float DegToRad(float degrees)
    {
        return (float)(degrees * (Math.PI / 180));
    }

    public static float CameraToRelative(float camRot, float playerRot)
    {
        camRot -= playerRot;

        while (camRot > Math.PI) { camRot -= (float)Math.Tau; }
        while (camRot < -Math.PI) { camRot += (float)Math.Tau; }

        return camRot;
    }

    public static float RelativeToCamera(float relRot, float playerRot)
    {
        relRot += playerRot;

        while (relRot > Math.PI) { relRot -= (float)Math.Tau; }
        while (relRot < -Math.PI) { relRot += (float)Math.Tau; }

        return relRot;
    }

    public static float AddPiRad(float angle)
    {
        angle += (float)Math.PI;
        while (angle > Math.PI) { angle -= (float)Math.Tau; }
        return angle;
    }

    public static float SubPiRad(float angle)
    {
        angle -= (float)Math.PI;
        while (angle < Math.PI) { angle += (float)Math.Tau; }
        return angle;
    }
}