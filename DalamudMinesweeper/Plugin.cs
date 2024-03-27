using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DalamudMinesweeper.Windows;

namespace DalamudMinesweeper;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/minesweeper";

    public DalamudPluginInterface _pluginInterface { get; init; }
    private ICommandManager _commandManager { get; init; }
    public Configuration _configuration { get; init; }
    public WindowSystem _windowSystem = new("Minesweeper");
    private ConfigWindow _configWindow { get; init; }
    private MainWindow _mainWindow { get; init; }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager)
    {
        _pluginInterface = pluginInterface;
        _commandManager = commandManager;

        _configuration = _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        _configuration.Initialize(_pluginInterface);

        _configWindow = new ConfigWindow(this);
        _mainWindow = new MainWindow(this, _configuration);
        
        _windowSystem.AddWindow(_configWindow);
        _windowSystem.AddWindow(_mainWindow);

        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Minesweeper window"
        });

        _pluginInterface.UiBuilder.Draw += DrawUI;
        _pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
        _windowSystem.RemoveAllWindows();
        
        _configWindow.Dispose();
        _mainWindow.Dispose();
        
        _commandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just display our main ui
        _mainWindow.IsOpen = true;
    }

    private void DrawUI()
    {
        _windowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        _configWindow.IsOpen = true;
    }
}
