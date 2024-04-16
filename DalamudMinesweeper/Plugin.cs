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
    private ScoresWindow _scoresWindow { get; init; }
    private MainWindow _mainWindow { get; init; }
    private ICommandManager _commandManager { get; init; }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager)
    {
        PluginInterface = pluginInterface;
        _commandManager = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        _configWindow = new ConfigWindow(this);
        _scoresWindow = new ScoresWindow(this);
        _mainWindow = new MainWindow(this, Configuration);
        
        WindowSystem.AddWindow(_configWindow);
        WindowSystem.AddWindow(_scoresWindow);
        WindowSystem.AddWindow(_mainWindow);

        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Minesweeper window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
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
        _mainWindow.IsOpen = !_mainWindow.IsOpen;
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    private void DrawMainUI()
    {
        _mainWindow.IsOpen = !_mainWindow.IsOpen;
    }

    public void DrawConfigUI()
    {
        _configWindow.IsOpen = !_configWindow.IsOpen;
    }

    public void DrawScoresUI()
    {
        _scoresWindow.IsOpen = !_scoresWindow.IsOpen;
    }
}
