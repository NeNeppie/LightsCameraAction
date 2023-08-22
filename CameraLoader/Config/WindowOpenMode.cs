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
        return value switch
        {
            WindowOpenMode.Manual => "Manually",
            WindowOpenMode.OnEnterGPose => "When entering GPose",
            WindowOpenMode.OnStartup => "On Startup",
            _ => value.ToString(),
        };
    }
}