using Dalamud.Plugin;
using Dalamud.Interface.Internal;
using System.IO;
using System;
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using DalamudMinesweeper.Game;

namespace DalamudMinesweeper;

public class ClassicSprites : IDisposable {
    private DalamudPluginInterface _pluginInterface { get; set; }
    private IDalamudTextureWrap Sheet { get; init; }
    private record SpriteData(Vector2 topLeftCoord, Vector2 sizePx);
    private readonly Dictionary<string, SpriteData> _spriteDict;

    public ClassicSprites(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        Sheet = LoadImage("spritesheet.png");

        _spriteDict = new Dictionary<string, SpriteData>
        {
            { "0", new SpriteData(new Vector2(40, 88), new Vector2(16, 16)) },
            { "1", new SpriteData(new Vector2(0, 88), new Vector2(16, 16)) },
            { "2", new SpriteData(new Vector2(0, 20), new Vector2(16, 16)) },
            { "3", new SpriteData(new Vector2(40, 0), new Vector2(16, 16)) },
            { "4", new SpriteData(new Vector2(40, 20), new Vector2(16, 16)) },
            { "5", new SpriteData(new Vector2(20, 0), new Vector2(16, 16)) },
            { "6", new SpriteData(new Vector2(60, 88), new Vector2(16, 16)) },
            { "7", new SpriteData(new Vector2(20, 88), new Vector2(16, 16)) },
            { "8", new SpriteData(new Vector2(20, 40), new Vector2(16, 16)) },
            { "Flag", new SpriteData(new Vector2(0, 40), new Vector2(16, 16)) },
            { "Hidden", new SpriteData(new Vector2(20, 20), new Vector2(16, 16)) },
            { "Mine1", new SpriteData(new Vector2(0, 0), new Vector2(16, 16)) },
            { "Mine2", new SpriteData(new Vector2(40, 40), new Vector2(16, 16)) },
            { "Smiley", new SpriteData(new Vector2(60, 0), new Vector2(24, 24)) },
            { "SmileyClicked", new SpriteData(new Vector2(0, 60), new Vector2(24, 24)) },
            { "SmileyDead", new SpriteData(new Vector2(28, 60), new Vector2(24, 24)) },
            { "SmileyShades", new SpriteData(new Vector2(60, 28), new Vector2(24, 24)) },
            { "SmileySoy", new SpriteData(new Vector2(60, 56), new Vector2(24, 24)) }
        };
    }

    public Vector2 TileSize => _spriteDict["0"].sizePx;
    public Vector2 SmileySize => _spriteDict["Smiley"].sizePx;

    public void DrawTile(ImDrawListPtr drawList, Cell cell, Vector2 cursorPos, int zoom = 1)
        => Draw(drawList, _spriteDict[CellToSpriteName(cell)], cursorPos, zoom);

    public void DrawSmiley(ImDrawListPtr drawList, string smileyName, Vector2 cursorPos, int zoom = 1)
        => Draw(drawList, _spriteDict[smileyName], cursorPos, zoom);

    private void Draw(ImDrawListPtr drawList, SpriteData sprite, Vector2 cursorPos, int zoom)
    {
        var uvMin = sprite.topLeftCoord / Sheet.Size;
        var uvMax = (sprite.topLeftCoord + sprite.sizePx) / Sheet.Size;

        drawList.AddImage(
            Sheet.ImGuiHandle,
            cursorPos,
            cursorPos + sprite.sizePx * zoom,
            uvMin,
            uvMax);
    }

    private static string CellToSpriteName(Cell cell)
    {
        if (!cell.isRevealed)
        {
            if (cell.isFlagged)
            {
                return "Flag";
            }
            return "Hidden";
        }
        return cell.contents switch
        {
            CellContents.Clear => "0",
            CellContents.Mine => "Mine1",
            CellContents.ExplodedMine => "Mine2",
            CellContents.Number => cell.numNeighbouringMines switch {
                < 1 or > 8 => throw new("Invalid number of mines in cell " + cell.numNeighbouringMines),
                _ => cell.numNeighbouringMines.ToString()
            },
            _ => throw new("Unknown cell contents.")
        };
    }

    public void Dispose()
    {
        Sheet.Dispose();
    }

    private IDalamudTextureWrap LoadImage(string path)
    {
        var fullPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, path);
        return _pluginInterface.UiBuilder.LoadImage(fullPath);
    }

}