using System.Numerics;
using DalamudMinesweeper.Game;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class Background
{
    public MinesweeperGame Game;
    private Configuration _configuration;
    private int _borderWidthPx;
    private readonly Vector2 _borderWidthPxVec2;

    private readonly uint[,] _aliasSwatch = new uint[3,3]
    {
        { Colours.White,   Colours.White,    Colours.MidGrey  },
        { Colours.White,   Colours.MidGrey,  Colours.DarkGrey },
        { Colours.MidGrey, Colours.DarkGrey, Colours.DarkGrey }
    };

    public Background(MinesweeperGame game, Configuration configuration, int borderWidthPx)
    {
        Game = game;
        _configuration = configuration;
        _borderWidthPx = borderWidthPx;
        _borderWidthPxVec2 = new Vector2(_borderWidthPx, _borderWidthPx);
    }
    
    public void Draw(Vector2 cursorPos, Vector2 headerHeightPx, int gridSquareSizePx)
    {
        var drawList = ImGui.GetWindowDrawList();

        var edgeBorderWidthPx = 3 * _configuration.Zoom;
        var boardDimensions = new Vector2(Game.Width, Game.Height);

        var bgTopLeft = cursorPos;
        var bgBottomRight = cursorPos + gridSquareSizePx*boardDimensions + 2*_borderWidthPxVec2*_configuration.Zoom + headerHeightPx;
        var bgTopRight = new Vector2(bgBottomRight.X, bgTopLeft.Y);
        var bgBottomLeft = new Vector2(bgTopLeft.X, bgBottomRight.Y);

        // Background colour
        drawList.AddRectFilled(bgTopLeft, bgBottomRight, Colours.MidGrey);

        // Edges
        drawList.AddRectFilled(bgTopLeft, bgBottomLeft + new Vector2(edgeBorderWidthPx, 0), Colours.White);
        drawList.AddRectFilled(bgTopLeft, bgTopRight + new Vector2(0, edgeBorderWidthPx), Colours.White);
        drawList.AddRectFilled(bgTopRight + new Vector2(-edgeBorderWidthPx, edgeBorderWidthPx), bgBottomRight, Colours.DarkGrey);
        drawList.AddRectFilled(bgBottomLeft + new Vector2(edgeBorderWidthPx, -edgeBorderWidthPx), bgBottomRight, Colours.DarkGrey);

        // Corner aliasing
        var aliasingCursor = new Vector2(bgBottomLeft.X, bgBottomLeft.Y - 3*_configuration.Zoom);
        DrawAliasing(drawList, aliasingCursor);

        aliasingCursor = new Vector2(bgTopRight.X - 3*_configuration.Zoom, bgTopRight.Y);
        DrawAliasing(drawList, aliasingCursor);
    }

    private void DrawAliasing(ImDrawListPtr drawList, Vector2 start)
    {
        var zoomedPixel = new Vector2(_configuration.Zoom, _configuration.Zoom);
        
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {  
                var colour = _aliasSwatch[y,x];
                var offset = new Vector2(_configuration.Zoom * x, _configuration.Zoom * y);
                drawList.AddRectFilled(
                    start + offset,
                    start + offset + zoomedPixel,
                    colour);
            }
        }
    }
}
