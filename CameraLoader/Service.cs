using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using CameraLoader.Config;
using CameraLoader.Game;

namespace CameraLoader;

internal class Service
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static ICondition Conditions { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;

    public static GameFunctions GameFunctions { get; set; } = null!;
    public static GPoseHooking GPoseHooking { get; set; } = null!;
    public static Configuration Config { get; set; } = null!;

    public static bool Initialize()
    {
        Config = (Configuration)PluginInterface.GetPluginConfig() ?? new Configuration();
        Config.Initialize();

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
