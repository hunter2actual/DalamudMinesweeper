using Dalamud.Configuration;
using Dalamud.Plugin;
using DalamudMinesweeper.Game;
using System;

namespace DalamudMinesweeper;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public int BoardWidth { get; set; } = 16;
    public int BoardHeight { get; set; } = 16;
    public int NumMines { get; set; } = 40;
    public int Zoom { get; set; } = 2;
    public bool DevMode { get; set; } = false;
    public bool NoGuess { get; set; } = false;
    public int NoGuessTimeoutMs { get; set; } = 1500;
    public bool RevealShortcut { get; set; } = false;
    public bool FlagShortcut { get; set; } = false;

    public Scores Scores { get; set; } = new Scores([]);

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? _pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
}