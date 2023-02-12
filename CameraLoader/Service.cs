using Dalamud.IoC;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game;

namespace CameraLoader;

internal class Service
{
    [PluginService] public static ClientState ClientState { get; private set; } = null!;
    [PluginService] public static ObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] public static SigScanner SigScanner { get; private set; } = null!;

    public static Configuration Config { get; set; } = null!;
}
