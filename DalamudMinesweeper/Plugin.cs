using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DalamudMinesweeper.Windows;
using Dalamud.IoC;

namespace DalamudMinesweeper;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public sealed class Service
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
    [PluginService] public static ITextureProvider TextureProvider { get; set; }
    [PluginService] public static ICommandManager CommandManager { get; set; }
}

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/minesweeper";

    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem = new("Minesweeper");
    private ConfigWindow _configWindow { get; init; }
    private ScoresWindow _scoresWindow { get; init; }
    private MainWindow _mainWindow { get; init; }

    public Plugin(
        IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(pluginInterface);

        _configWindow = new ConfigWindow(this);
        _scoresWindow = new ScoresWindow(this);
        _mainWindow = new MainWindow(this, Configuration, Service.TextureProvider);
        
        WindowSystem.AddWindow(_configWindow);
        WindowSystem.AddWindow(_scoresWindow);
        WindowSystem.AddWindow(_mainWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Minesweeper window"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenMainUi += DrawMainUI;
        pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        
        _configWindow.Dispose();
        _mainWindow.Dispose();
        
        Service.CommandManager.RemoveHandler(CommandName);
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
