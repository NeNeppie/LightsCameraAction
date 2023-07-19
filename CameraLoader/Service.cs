using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.IoC;
using Dalamud.Plugin;

using CameraLoader.Config;
using CameraLoader.Game;

namespace CameraLoader;

internal class Service
{
    [PluginService] public static ClientState ClientState { get; private set; } = null!;
    [PluginService] public static ObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static SigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static Condition Conditions { get; private set; } = null!;

    public static GameFunctions GameFunctions { get; set; } = null!;
    public static GPoseHooking GPoseHooking { get; set; } = null!;
    public static Configuration Config { get; set; } = null!;

    public static bool Initialize(DalamudPluginInterface pi)
    {
        Config = (Configuration)pi.GetPluginConfig() ?? new Configuration();
        Config.Initialize(pi);

        GPoseHooking = new GPoseHooking();
        GameFunctions = new GameFunctions();

        if (Config.WindowOpenMode == WindowOpenMode.OnStartup)
        {
            return true;
        }
        return false;
    }

    public static void Dispose()
    {
        GPoseHooking.Dispose();
        GameFunctions.Dispose();
    }
}
