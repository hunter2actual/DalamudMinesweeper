using System;
using System.Numerics;
using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sprites;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class Header
{
    public MinesweeperGame Game { get; set; }
    private readonly TileSprites _tileSprites;
    private readonly NumberSprites _numberSprites;
    private readonly Configuration _configuration;
    private Action _initialiseGame;
    private bool _smileyClicked;

    public Header(MinesweeperGame game, TileSprites tileSprites, NumberSprites numberSprites, Configuration configuration, Action initialiseGame)
    {
        Game = game;
        _tileSprites = tileSprites;
        _numberSprites = numberSprites;
        _configuration = configuration;
        _initialiseGame = initialiseGame;
    }

    public void Draw(Vector2 start, float headerWidth)
    {
        var mousePos = ImGui.GetMousePos();
        var drawList = ImGui.GetWindowDrawList();

        string smileyToDraw = "Smiley";
        var smileySize = _tileSprites.SmileySize * _configuration.Zoom;
        float leftPadding = (float) ((headerWidth - smileySize.X) * 0.5);
        var cursorPos = start + Vector2.UnitX*leftPadding;

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

        if (Game.Width >= 8)
        {
            cursorPos += Vector2.UnitX*30*_configuration.Zoom;
            DrawNumUnflaggedMines(cursorPos, drawList);
            cursorPos += Vector2.UnitX*40*_configuration.Zoom;
            // DrawTimer(cursorPos, drawList);
        }
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