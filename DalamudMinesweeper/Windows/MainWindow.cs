using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DalamudMinesweeper.Components;
using DalamudMinesweeper.Game;
using ImGuiNET;

namespace DalamudMinesweeper.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly ClassicSprites _classicSprites;
    private readonly Configuration _configuration;
    private MinesweeperGame _game;
    private Vector2 _boardDimensions;

    // Pixel sizes
    private readonly int _dalamudWindowPaddingPx = 8;
    private int _gridSquareSizePx;
    private Vector2 _gridSquareSizePxVec2;
    private readonly int _borderWidthPx = 9;
    private readonly Vector2 _borderWidthPxVec2 = new(9, 9);
    private readonly int _titleBarHeightPx = 26;
    private readonly Vector2 _footerHeightPxVec2 = new(0, 26);

    // Renderable components
    private GameBoard _gameBoard;
    private Header _header;
    private Footer _footer;

    public MainWindow(Plugin plugin, Configuration configuration): base("Minesweeper",
            ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoDocking)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        _configuration = configuration;
        _classicSprites = new ClassicSprites(plugin.PluginInterface);
        _gridSquareSizePxVec2 = new Vector2(0, 0);

        _game = InitialiseGame();
        _gameBoard = new GameBoard(_game, _classicSprites, _configuration);
        _header = new Header(_game, _classicSprites, _configuration, () => InitialiseGame());
        _footer = new Footer(_configuration, plugin.DrawConfigUI);
    }

    public void Dispose()
    {
        _classicSprites.Dispose();
    }

    public override void Draw()
    {
        // Calculate element sizes
        var windowPos = ImGui.GetWindowPos();
        var headerHeightPx = _classicSprites.Smiley.Height * _configuration.Zoom;

        _gridSquareSizePx = _classicSprites.Tile0.Width * _configuration.Zoom;
        _gridSquareSizePxVec2.X = _gridSquareSizePxVec2.Y = _gridSquareSizePx;
        _boardDimensions = new Vector2(_game.Width, _game.Height);
        var windowWidthPx = _gridSquareSizePx*_boardDimensions.X + 2*_borderWidthPx*_configuration.Zoom + 2*_dalamudWindowPaddingPx;
        var windowHeightPx = _gridSquareSizePx*_boardDimensions.Y + 2*_borderWidthPx*_configuration.Zoom + 2*_dalamudWindowPaddingPx
                            + _footerHeightPxVec2.Y + headerHeightPx + _titleBarHeightPx + 8;
        var windowSize = new Vector2(windowWidthPx, windowHeightPx);
        ImGui.SetWindowSize(windowSize);

        // Get window corner coords (relative to window)
        // Content region is 8px padded from window
        var topLeft = ImGui.GetWindowContentRegionMin();
        var bottomRight = ImGui.GetWindowContentRegionMax();
        var topRight = new Vector2(bottomRight.X, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

        // Cover everything except the footer in an anticlick field to stop window movement
        ImGui.InvisibleButton("anticlick", bottomRight - topLeft - _footerHeightPxVec2);
        
        // draw everything
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = windowPos + topLeft;

        DrawBackground(drawList, cursorPos, new Vector2(0, headerHeightPx));

        cursorPos += _borderWidthPxVec2 * _configuration.Zoom;

        var headerWidth = topRight.X - topLeft.X - 2*_borderWidthPx*_configuration.Zoom;
        _header.Draw(cursorPos, headerWidth);
        
        cursorPos += new Vector2(0, headerHeightPx);

        _gameBoard.Draw(cursorPos);
        
        _footer.Draw(bottomLeft - _footerHeightPxVec2);
    }

    private void DrawBackground(ImDrawListPtr drawList, Vector2 cursorPos, Vector2 headerHeightPx)
    {
        var edgeBorderWidthPx = 3 * _configuration.Zoom;

        var bgTopLeft = cursorPos;
        var bgBottomRight = cursorPos + _gridSquareSizePx*_boardDimensions + 2*_borderWidthPxVec2*_configuration.Zoom + headerHeightPx;
        var bgTopRight = new Vector2(bgBottomRight.X, bgTopLeft.Y);
        var bgBottomLeft = new Vector2(bgTopLeft.X, bgBottomRight.Y);

        // Background colour
        drawList.AddRectFilled(bgTopLeft, bgBottomRight, Colours.MidGrey);

        // Edges
        drawList.AddRectFilled(bgTopLeft, bgBottomLeft + new Vector2(edgeBorderWidthPx, 0), Colours.White);
        drawList.AddRectFilled(bgTopLeft, bgTopRight + new Vector2(0, edgeBorderWidthPx), Colours.White);
        drawList.AddRectFilled(bgTopRight + new Vector2(-edgeBorderWidthPx, edgeBorderWidthPx), bgBottomRight, Colours.DarkGrey);
        drawList.AddRectFilled(bgBottomLeft + new Vector2(edgeBorderWidthPx, -edgeBorderWidthPx), bgBottomRight, Colours.DarkGrey);
    }

    private MinesweeperGame InitialiseGame()
    {
        _game = new MinesweeperGame(
            _configuration.BoardWidth,
            _configuration.BoardHeight,
            _configuration.NumMines);

        if (_header is not null) 
        {
            _header.Game = _game;
        }
        if (_gameBoard is not null) 
        {
            _gameBoard.Game = _game;
        }

        return _game;
    }
}
