using Dalamud.Plugin;
using Dalamud.Logging;
using Dalamud.Game.Command;
using Dalamud.Game.ClientState;
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

    public CameraLoader(
        DalamudPluginInterface pluginInterface,
        CommandManager commands,
        ClientState clientState)
    {
        this._pluginInterface = pluginInterface;
        this._commandManager = new PluginCommandManager<CameraLoader>(this, commands);

        // Initialize the UI
        this._windowSystem = new WindowSystem(this.Name);

        _window = this._pluginInterface.Create<PluginWindow>(clientState);
        if (_window is not null)
        {
            this._windowSystem.AddWindow(_window);
        }

        this._pluginInterface.UiBuilder.DisableAutomaticUiHide = true;
        this._pluginInterface.UiBuilder.Draw += this._windowSystem.Draw;
    }

    [Command("/cameracfg")]
    [HelpMessage("Opens CameraLoader's config menu")]
    public unsafe void OnCommand(string command, string args)
    {
        _window.Toggle();
    }

    #region IDisposable Support
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        this._commandManager.Dispose();

        //this._pluginInterface.SavePluginConfig(this.config);

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