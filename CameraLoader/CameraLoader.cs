using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using CameraLoader.Attributes;
using System;

namespace CameraLoader;

public class CameraLoader : IDalamudPlugin
{
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly PluginCommandManager<CameraLoader> _commandManager;
    private readonly WindowSystem _windowSystem;
    private readonly Configuration _config;

    private PluginWindow _window;
    public string Name => "CameraLoader";

    public CameraLoader(
        DalamudPluginInterface pluginInterface,
        CommandManager commands,
        ClientState clientState,
        ObjectTable objectTable,
        SigScanner sigScanner)
    {
        this._pluginInterface = pluginInterface;

        // Load commands
        this._commandManager = new PluginCommandManager<CameraLoader>(this, commands);

        // Get or create a configuration object
        this._config = (Configuration)this._pluginInterface.GetPluginConfig() ?? new Configuration();
        this._config.Initialize(pluginInterface);

        // Initialize the UI
        this._windowSystem = new WindowSystem(this.Name);

        _window = this._pluginInterface.Create<PluginWindow>(clientState, objectTable, sigScanner, _config);
        if (_window is not null)
        {
            this._windowSystem.AddWindow(_window);
        }

        this._pluginInterface.UiBuilder.DisableGposeUiHide = true;
        this._pluginInterface.UiBuilder.Draw += this._windowSystem.Draw;
    }

    [Command("/cameraloader")]
    [Aliases("/cam")]
    [HelpMessage("Toggles CameraLoader's main window")]
    public unsafe void OnCommand(string command, string args)
    {
        _window.Toggle();
    }

    #region IDisposable Support
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        this._commandManager.Dispose();

        this._pluginInterface.SavePluginConfig(this._config);

        this._pluginInterface.UiBuilder.Draw -= this._windowSystem.Draw;
        this._windowSystem.RemoveAllWindows();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}