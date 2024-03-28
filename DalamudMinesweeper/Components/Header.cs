using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using DalamudMinesweeper.Game;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class Header
{
    public MinesweeperGame Game { get; set; }
    private readonly ClassicSprites _classicSprites;
    private readonly Configuration _configuration;
    private Action _initialiseGame;
    private bool _smileyClicked;

    public Header(MinesweeperGame game, ClassicSprites classicSprites, Configuration configuration, Action initialiseGame)
    {
        Game = game;
        _classicSprites = classicSprites;
        _configuration = configuration;
        _initialiseGame = initialiseGame;
    }

    public void Draw(Vector2 start, float headerWidth)
    {
        var mousePos = ImGui.GetMousePos();
        var drawList = ImGui.GetWindowDrawList();

        IDalamudTextureWrap smileyToDraw = _classicSprites.Smiley;
        var smileySize = smileyToDraw.Size * _configuration.Zoom;
        float leftPadding = (float) ((headerWidth - smileySize.X) * 0.5);
        var cursorPos = start + new Vector2(leftPadding, 0);

        if (Game.GameState == GameState.Victorious)
        {
            smileyToDraw = _classicSprites.SmileyShades;
        }
        else if (Game.GameState == GameState.Boom)
        {
            smileyToDraw = _classicSprites.SmileyDead;
        }

        if (MouseInSquare(mousePos, cursorPos, (int) smileySize.X) 
            && ImGui.IsWindowFocused())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                smileyToDraw = _classicSprites.SmileyClicked;
                _smileyClicked = true;
            }
            else if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && _smileyClicked)
            {
                smileyToDraw = _classicSprites.SmileyClicked;
            }
            else if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _smileyClicked)
            {
                smileyToDraw = _classicSprites.Smiley;
                _smileyClicked = false;
                _initialiseGame();
            }
        }
        
        // TODO soyface when clicking on game

        drawList.AddImage(smileyToDraw.ImGuiHandle, cursorPos, cursorPos + smileySize);
    }

    private bool MouseInSquare(Vector2 mousePos, Vector2 cursorPos, int squareSize)
        => mousePos.X > cursorPos.X
        && mousePos.X <= cursorPos.X + squareSize
        && mousePos.Y > cursorPos.Y
        && mousePos.Y <= cursorPos.Y + squareSize;
}