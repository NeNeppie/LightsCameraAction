namespace CameraLoader.Config;

public enum WindowOpenMode
{
    Manual,
    OnEnterGPose,
    OnStartup
}

static class WindowOpenModeEx
{
    public static string GetDescription(this WindowOpenMode value)
    {
        switch (value)
        {
            case WindowOpenMode.Manual:
                return "Manually";
            case WindowOpenMode.OnEnterGPose:
                return "When entering GPose";
            case WindowOpenMode.OnStartup:
                return "On Startup";
            default:
                return value.ToString();
        }
    }
}