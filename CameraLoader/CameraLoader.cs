using Dalamud.Plugin;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using CameraLoader.Attributes;
using System;

namespace CameraLoader;

public class CameraLoader : IDalamudPlugin
{
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly PluginCommandManager<CameraLoader> _commandManager;
    private readonly WindowSystem _windowSystem;

    private PluginWindow _window;
    public string Name => "CameraLoader";

    public CameraLoader(DalamudPluginInterface pluginInterface, CommandManager commands)
    {
        this._pluginInterface = pluginInterface;
        this._pluginInterface.Create<Service>();

        // Load commands
        this._commandManager = new PluginCommandManager<CameraLoader>(this, commands);

        // Get or create a configuration object
        Service.Config = (Configuration)this._pluginInterface.GetPluginConfig() ?? new Configuration();
        Service.Config.Initialize(this._pluginInterface);

        // Initialize the UI
        _windowSystem = new WindowSystem(this.Name);
        _window = this._pluginInterface.Create<PluginWindow>();
        _windowSystem.AddWindow(_window);

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

        this._pluginInterface.SavePluginConfig(Service.Config);

        this._pluginInterface.UiBuilder.Draw -= this._window.Draw;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}