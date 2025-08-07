using System.Numerics;
using DalamudMinesweeper.Game;
using Dalamud.Bindings.ImGui;

namespace DalamudMinesweeper.Components;

public class Background
{
    public MinesweeperGame Game;
    private Configuration _configuration;
    private int _borderWidthPx;
    private readonly Vector2 _borderWidthPxVec2;

    private readonly uint[,] _outerCornerSwatch = new uint[3,3]
    {
        { Colours.White,   Colours.White,    Colours.MidGrey  },
        { Colours.White,   Colours.MidGrey,  Colours.DarkGrey },
        { Colours.MidGrey, Colours.DarkGrey, Colours.DarkGrey }
    };

    private readonly uint[,] _innerCornerSwatch = new uint[3,3]
    {
        { Colours.DarkGrey, Colours.DarkGrey, Colours.MidGrey },
        { Colours.DarkGrey, Colours.MidGrey,  Colours.White   },
        { Colours.MidGrey,  Colours.White,    Colours.White   }
    };

    public Background(MinesweeperGame game, Configuration configuration, int borderWidthPx)
    {
        Game = game;
        _configuration = configuration;
        _borderWidthPx = borderWidthPx;
        _borderWidthPxVec2 = new Vector2(_borderWidthPx, _borderWidthPx);
    }
    
    public void Draw(Vector2 cursorPos, Vector2 headerHeightPxVec2, int gridSquareSizePx)
    {
        var drawList = ImGui.GetWindowDrawList();

        var borderEdgeWidthPx = 3 * _configuration.Zoom;
        var boardDimensions = new Vector2(Game.Width, Game.Height);
        var boardSizePxVec2 = gridSquareSizePx*boardDimensions; // (already zoomed before being passed in)

        var bgTopLeft = cursorPos;
        var bgBottomRight = cursorPos + boardSizePxVec2 + 2*_borderWidthPxVec2*_configuration.Zoom
            + headerHeightPxVec2 + Vector2.UnitY*(_borderWidthPx-1)*_configuration.Zoom;
        var bgTopRight = new Vector2(bgBottomRight.X, bgTopLeft.Y);
        var bgBottomLeft = new Vector2(bgTopLeft.X, bgBottomRight.Y);

        // Background colour
        drawList.AddRectFilled(bgTopLeft, bgBottomRight, Colours.MidGrey);

        // Outer edges
        drawList.AddRectFilled(bgTopLeft, bgBottomLeft + Vector2.UnitX*borderEdgeWidthPx, Colours.White);
        drawList.AddRectFilled(bgTopLeft, bgTopRight + Vector2.UnitY*borderEdgeWidthPx, Colours.White);
        drawList.AddRectFilled(bgTopRight + new Vector2(-borderEdgeWidthPx, borderEdgeWidthPx), bgBottomRight, Colours.DarkGrey);
        drawList.AddRectFilled(bgBottomLeft + new Vector2(borderEdgeWidthPx, -borderEdgeWidthPx), bgBottomRight, Colours.DarkGrey);

        // Outer corner aliasing
        var aliasingCursor = new Vector2(bgBottomLeft.X, bgBottomLeft.Y - borderEdgeWidthPx);
        DrawAliasing(drawList, aliasingCursor, _outerCornerSwatch);

        aliasingCursor = new Vector2(bgTopRight.X - borderEdgeWidthPx, bgTopRight.Y);
        DrawAliasing(drawList, aliasingCursor, _outerCornerSwatch);


        // Inner edges around board
        // Technically these vars are the corners of board + the edge we are drawing
        var boardTopLeft = bgTopLeft + headerHeightPxVec2 + _borderWidthPxVec2*_configuration.Zoom
            - Vector2.One*borderEdgeWidthPx + Vector2.UnitY*(_borderWidthPx-1)*_configuration.Zoom;
        var boardBottomRight = boardTopLeft + boardSizePxVec2 + 2*Vector2.One*borderEdgeWidthPx;
        var boardBottomLeft = new Vector2(boardTopLeft.X, boardBottomRight.Y);
        var boardTopRight = new Vector2(boardBottomRight.X, boardTopLeft.Y);

        drawList.AddRectFilled(
            boardTopLeft,
            boardBottomLeft + Vector2.UnitX*borderEdgeWidthPx - Vector2.UnitY*borderEdgeWidthPx,
            Colours.DarkGrey);
        drawList.AddRectFilled(
            boardTopLeft,
            boardTopRight - Vector2.UnitX*borderEdgeWidthPx + Vector2.UnitY*borderEdgeWidthPx,
            Colours.DarkGrey);
        drawList.AddRectFilled(
            boardTopRight - Vector2.UnitX*borderEdgeWidthPx + Vector2.UnitY*borderEdgeWidthPx,
            boardBottomRight,
            Colours.White);
        drawList.AddRectFilled(
            boardBottomLeft + Vector2.UnitX*borderEdgeWidthPx - Vector2.UnitY*borderEdgeWidthPx,
            boardBottomRight,
            Colours.White);

        // Inner corner aliasing
        aliasingCursor = boardBottomLeft - Vector2.UnitY*borderEdgeWidthPx;
        DrawAliasing(drawList, aliasingCursor, _innerCornerSwatch);

        aliasingCursor = boardTopRight - Vector2.UnitX*borderEdgeWidthPx;
        DrawAliasing(drawList, aliasingCursor, _innerCornerSwatch);
    }

    private void DrawAliasing(ImDrawListPtr drawList, Vector2 start, uint[,] swatch)
    {
        var zoomedPixel = new Vector2(_configuration.Zoom, _configuration.Zoom);
        
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {  
                var colour = swatch[y,x];
                var offset = new Vector2(_configuration.Zoom * x, _configuration.Zoom * y);
                drawList.AddRectFilled(
                    start + offset,
                    start + offset + zoomedPixel,
                    colour);
            }
        }
    }
}
