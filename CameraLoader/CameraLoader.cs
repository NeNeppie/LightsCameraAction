using System;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using CameraLoader.UI;

namespace CameraLoader;

public class CameraLoader : IDalamudPlugin
{
    private readonly WindowSystem _windowSystem;

    private PluginWindow _window;
    public static string Name => "Lights, Camera, Action!";

    public CameraLoader(IDalamudPluginInterface pluginInterface, ICommandManager commands)
    {
        pluginInterface.Create<Service>();
        bool windowState = Service.Initialize();

        this._windowSystem = new WindowSystem(Name);
        this._window = Service.PluginInterface.Create<PluginWindow>();
        this._window.IsOpen = windowState;
        this._windowSystem.AddWindow(_window);

        Service.CommandManager.AddHandler("/lca", new Dalamud.Game.Command.CommandInfo(OnCommand)
        {
            HelpMessage = "Toggles LCAction's main window"
        });

        Service.PluginInterface.UiBuilder.Draw += this._windowSystem.Draw;
        Service.PluginInterface.UiBuilder.DisableGposeUiHide = true;
    }

    private void OnCommand(string command, string args)
    {
        this._window.Toggle();
    }

    #region IDisposable Support
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        Service.PluginInterface.SavePluginConfig(Service.Config);
        Service.PluginInterface.UiBuilder.Draw -= this._window.Draw;
    }

    public void Dispose()
    {
        this.Dispose(true);
        Service.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}
