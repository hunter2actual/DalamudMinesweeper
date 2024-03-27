using Dalamud.Plugin;
using Dalamud.Interface.Internal;
using System.IO;
using System;

namespace DalamudMinesweeper;

public class ClassicSprites : IDisposable {
    private DalamudPluginInterface _pluginInterface { get; set; }

    public ClassicSprites(DalamudPluginInterface pluginInterface) {
        _pluginInterface = pluginInterface;
        Tile0 = LoadImage("0.png");
        Tile1 = LoadImage("1.png");
        Tile2 = LoadImage("2.png");
        Tile3 = LoadImage("3.png");
        Tile4 = LoadImage("4.png");
        Tile5 = LoadImage("5.png");
        Tile6 = LoadImage("6.png");
        Tile7 = LoadImage("7.png");
        Tile8 = LoadImage("8.png");
        TileFlag = LoadImage("Flag.png");
        TileHidden = LoadImage("Hidden.png");
        TileMine = LoadImage("Mine1.png");
        TileMineBoom = LoadImage("Mine2.png");
    }

    public void Dispose() {
        Tile0.Dispose();
        Tile1.Dispose();
        Tile2.Dispose();
        Tile3.Dispose();
        Tile4.Dispose();
        Tile5.Dispose();
        Tile6.Dispose();
        Tile7.Dispose();
        Tile8.Dispose();
        TileFlag.Dispose();
        TileHidden.Dispose();
        TileMine.Dispose();
        TileMineBoom.Dispose();
    }

    private IDalamudTextureWrap LoadImage(string path) {
        var fullPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, path);
        return _pluginInterface.UiBuilder.LoadImage(fullPath);
    }

    public IDalamudTextureWrap Tile0 { get; init; }
    public IDalamudTextureWrap Tile1 { get; init; }
    public IDalamudTextureWrap Tile2 { get; init; }
    public IDalamudTextureWrap Tile3 { get; init; }
    public IDalamudTextureWrap Tile4 { get; init; }
    public IDalamudTextureWrap Tile5 { get; init; }
    public IDalamudTextureWrap Tile6 { get; init; }
    public IDalamudTextureWrap Tile7 { get; init; }
    public IDalamudTextureWrap Tile8 { get; init; }
    public IDalamudTextureWrap TileFlag { get; init; }
    public IDalamudTextureWrap TileHidden { get; init; }
    public IDalamudTextureWrap TileMine { get; init; }
    public IDalamudTextureWrap TileMineBoom { get; init; }
}