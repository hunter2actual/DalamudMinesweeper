using Dalamud.Plugin;
using Dalamud.Interface.Internal;
using System.IO;
using System;
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;

namespace DalamudMinesweeper.Sprites;

public class NumberSprites : IDisposable
{
    private DalamudPluginInterface _pluginInterface { get; set; }
    private IDalamudTextureWrap[] Sheets { get; init; }
    private record SpriteData(Vector2 topLeftCoord, Vector2 sizePx);
    private readonly Dictionary<char, SpriteData> _spriteDict;

    public NumberSprites(DalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;

        Sheets =
        [
            LoadImage("numbers_1x.png"),
            LoadImage("numbers_2x.png"),
            LoadImage("numbers_3x.png"),
            LoadImage("numbers_4x.png"),
            LoadImage("numbers_5x.png"),
        ];

        var numberSpriteSize = new Vector2(13, 23);

        _spriteDict = new Dictionary<char, SpriteData>
        {
            { '0', new SpriteData(new Vector2(2, 2), numberSpriteSize) },
            { '1', new SpriteData(new Vector2(17, 2), numberSpriteSize) },
            { '2', new SpriteData(new Vector2(32, 2), numberSpriteSize) },
            { '3', new SpriteData(new Vector2(47, 2), numberSpriteSize) },
            { '4', new SpriteData(new Vector2(62, 2), numberSpriteSize) },
            { '5', new SpriteData(new Vector2(77, 2), numberSpriteSize) },
            { '6', new SpriteData(new Vector2(92, 2), numberSpriteSize) },
            { '7', new SpriteData(new Vector2(107, 2), numberSpriteSize) },
            { '8', new SpriteData(new Vector2(122, 2), numberSpriteSize) },
            { '9', new SpriteData(new Vector2(137, 2), numberSpriteSize) },
            { '-', new SpriteData(new Vector2(152, 2), numberSpriteSize) },
            { ' ', new SpriteData(new Vector2(167, 2), numberSpriteSize) },
        };
    }

    public Vector2 NumberSize => _spriteDict[' '].sizePx;

    public void DrawNumber(ImDrawListPtr drawList, char digit, Vector2 cursorPos, int zoom = 1)
        => Draw(drawList, _spriteDict[digit], cursorPos, zoom);

    private void Draw(ImDrawListPtr drawList, SpriteData sprite, Vector2 cursorPos, int zoom)
    {
        var sheet = Sheets[zoom - 1];

        var uvMin = sprite.topLeftCoord * zoom / sheet.Size;
        var uvMax = (sprite.topLeftCoord + sprite.sizePx) * zoom / sheet.Size;

        drawList.AddImage(
            sheet.ImGuiHandle,
            cursorPos,
            cursorPos + sprite.sizePx * zoom,
            uvMin,
            uvMax);
    }

    public void Dispose()
    {
        foreach (var sheet in Sheets)
        {
            sheet.Dispose();
        }
    }

    private IDalamudTextureWrap LoadImage(string path)
    {
        var fullPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, path);
        return _pluginInterface.UiBuilder.LoadImage(fullPath);
    }
}