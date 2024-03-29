using Dalamud.Configuration;
using Dalamud.Plugin;
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
