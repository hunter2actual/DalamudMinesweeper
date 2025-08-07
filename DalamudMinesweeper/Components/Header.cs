using System;
using System.Numerics;
using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sprites;
using Dalamud.Bindings.ImGui;

namespace DalamudMinesweeper.Components;

public class Header
{
    public MinesweeperGame Game { get; set; }
    private readonly TileSprites _tileSprites;
    private readonly NumberSprites _numberSprites;
    private readonly Configuration _configuration;
    private Action _initialiseGame;
    private bool _smileyClicked;
    private readonly int _timerPadding = 4;
    private readonly uint[,] _cornerSwatch = new uint[2,2]
    {
        { Colours.DarkGrey, Colours.MidGrey },
        { Colours.MidGrey,  Colours.White   }
    };

    public Header(MinesweeperGame game, TileSprites tileSprites, NumberSprites numberSprites, Configuration configuration, Action initialiseGame)
    {
        Game = game;
        _tileSprites = tileSprites;
        _numberSprites = numberSprites;
        _configuration = configuration;
        _initialiseGame = initialiseGame;
    }

    public void Draw(Vector2 start, int headerWidthPx, int headerHeightPx)
    {
        var drawList = ImGui.GetWindowDrawList();

        DrawBorders(start, drawList, headerWidthPx, headerHeightPx);

        var cursorPos = start + 3*_configuration.Zoom*Vector2.UnitY;

        DrawSmiley(cursorPos, drawList, headerWidthPx);

        if (Game.Width >= 8)
        {
            cursorPos += Vector2.UnitX*_timerPadding*_configuration.Zoom;
            DrawNumUnflaggedMines(cursorPos, drawList);

            cursorPos.X = start.X + (headerWidthPx - (3 + _timerPadding + 3*_numberSprites.NumberSize.X)*_configuration.Zoom);
            DrawTimer(cursorPos, drawList);
        }
    }

    private void DrawBorders(Vector2 start, ImDrawListPtr drawList, int headerWidthPx, int headerHeightPx)
    {
        var borderEdgeWidthPx = 2 * _configuration.Zoom;

        var topLeft = start - 3*_configuration.Zoom*Vector2.One;
        var bottomRight = start + new Vector2(headerWidthPx, headerHeightPx);
        var topRight = new Vector2(bottomRight.X, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

        drawList.AddRectFilled(topLeft, bottomLeft + Vector2.UnitX*borderEdgeWidthPx, Colours.DarkGrey);
        drawList.AddRectFilled(topLeft, topRight + Vector2.UnitY*borderEdgeWidthPx, Colours.DarkGrey);
        drawList.AddRectFilled(bottomLeft + Vector2.UnitX*borderEdgeWidthPx, bottomRight + Vector2.One*borderEdgeWidthPx, Colours.White);
        drawList.AddRectFilled(topRight + Vector2.UnitY*borderEdgeWidthPx, bottomRight + Vector2.One*borderEdgeWidthPx, Colours.White);

        // Inner corner aliasing
        DrawAliasing(drawList, bottomLeft);
        DrawAliasing(drawList, topRight);
    }

    private void DrawAliasing(ImDrawListPtr drawList, Vector2 start)
    {
        var zoomedPixel = new Vector2(_configuration.Zoom, _configuration.Zoom);
        
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {  
                var colour = _cornerSwatch[y,x];
                var offset = new Vector2(_configuration.Zoom * x, _configuration.Zoom * y);
                drawList.AddRectFilled(
                    start + offset,
                    start + offset + zoomedPixel,
                    colour);
            }
        }
    }

    private void DrawSmiley(Vector2 start, ImDrawListPtr drawList, int headerWidthPx)
    {
        var mousePos = ImGui.GetMousePos();
        string smileyToDraw = "Smiley";
        var smileySize = _tileSprites.SmileySize * _configuration.Zoom;
        int leftPadding = (int) ((headerWidthPx - smileySize.X) * 0.5);
        var cursorPos = start + Vector2.UnitX*leftPadding;

        // var topLeft = cursorPos;
        // var bottomRight = cursorPos + smileySize;
        // drawList.AddRectFilled(cursorPos, bottomRight, Colours.DarkGrey);

        // cursorPos += _configuration.Zoom*Vector2.One;

        if (Game.GameState == GameState.Victorious)
        {
            smileyToDraw = "SmileyShades";
        }
        else if (Game.GameState == GameState.Boom)
        {
            smileyToDraw = "SmileyDead";
        }

        if (MouseInSquare(mousePos, cursorPos, (int) smileySize.X) 
            && ImGui.IsWindowFocused())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                smileyToDraw = "SmileyClicked";
                _smileyClicked = true;
            }
            else if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && _smileyClicked)
            {
                smileyToDraw = "SmileyClicked";
            }
            else if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _smileyClicked)
            {
                smileyToDraw = "Smiley";
                _smileyClicked = false;
                _initialiseGame();
            }
        }
        
        // TODO soyface when clicking on game
        _tileSprites.DrawSmiley(drawList, smileyToDraw, cursorPos, _configuration.Zoom);
    }

    private static bool MouseInSquare(Vector2 mousePos, Vector2 cursorPos, int squareSize)
        => mousePos.X > cursorPos.X
        && mousePos.X <= cursorPos.X + squareSize
        && mousePos.Y > cursorPos.Y
        && mousePos.Y <= cursorPos.Y + squareSize;

    private void DrawNumUnflaggedMines(Vector2 start, ImDrawListPtr drawList)
        => DrawCounter(start, drawList, Game.NumUnflaggedMines());

    private void DrawTimer(Vector2 start, ImDrawListPtr drawList)
        => DrawCounter(start, drawList, Game.ElapsedGameTime);

    private void DrawCounter(Vector2 start, ImDrawListPtr drawList, int? number)
    {
        var cursorPos = start;
        var shadingBoxSize = (_numberSprites.NumberSize + 2*Vector2.UnitX*_numberSprites.NumberSize.X + Vector2.One)*_configuration.Zoom;

        drawList.AddRectFilled(
            cursorPos,
            cursorPos + shadingBoxSize,
            Colours.DarkGrey);
        
        cursorPos += Vector2.One * _configuration.Zoom;

        drawList.AddRectFilled(
            cursorPos,
            cursorPos + shadingBoxSize,
            Colours.White);

        foreach (char c in NumToSpriteString(number))
        {
            _numberSprites.DrawNumber(drawList, c, cursorPos, _configuration.Zoom);
            cursorPos += Vector2.UnitX * _configuration.Zoom * _numberSprites.NumberSize.X;
        }
    }
    
    private static string NumToSpriteString(int? number)
    {
        if (number is null) return "   ";
        if (number < -99) number = -99;
        if (number > 999) number = 999;

        return number.ToString()!.PadLeft(3);
    }
}   