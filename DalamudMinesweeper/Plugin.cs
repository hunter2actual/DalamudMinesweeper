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

    public DalamudPluginInterface PluginInterface { get; init; }
    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem = new("Minesweeper");
    private ConfigWindow _configWindow { get; init; }
    private ICommandManager _commandManager { get; init; }
    private MainWindow _mainWindow { get; init; }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager)
    {
        PluginInterface = pluginInterface;
        _commandManager = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        _configWindow = new ConfigWindow(this);
        _mainWindow = new MainWindow(this, Configuration);
        
        WindowSystem.AddWindow(_configWindow);
        WindowSystem.AddWindow(_mainWindow);

        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Minesweeper window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        
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
        WindowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        _configWindow.IsOpen = true;
    }
}
