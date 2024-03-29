using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Internal;
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
    private int _gridSquareSizePx;
    private Vector2 _gridSquareSizePxVec2;
    private Vector2 _boardDimensions;
    private readonly Vector2 _boardPaddingPx = new Vector2(5, 5);
    private readonly int _dalamudWindowPaddingPx = 8;
    private readonly int _titleBarHeightPx = 26;
    private readonly Vector2 _footerHeightPx = new Vector2(0, 26);

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
        var windowWidthPx = _gridSquareSizePx*_boardDimensions.X + 2*_boardPaddingPx.X + 2*_dalamudWindowPaddingPx;
        var windowHeightPx = _gridSquareSizePx*_boardDimensions.Y + 2*_boardPaddingPx.Y + 2*_dalamudWindowPaddingPx
                            + _footerHeightPx.Y + headerHeightPx + _titleBarHeightPx + 8;
        var windowSize = new Vector2(windowWidthPx, windowHeightPx);
        ImGui.SetWindowSize(windowSize);

        // Get window corner coords (relative to window)
        // Content region is 8px padded from window
        var topLeft = ImGui.GetWindowContentRegionMin();
        var bottomRight = ImGui.GetWindowContentRegionMax();
        var topRight = new Vector2(bottomRight.X, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

        // Cover everything except the footer in an anticlick field
        ImGui.InvisibleButton("anticlick", bottomRight - topLeft - _footerHeightPx);
        
        // draw everything
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = windowPos + topLeft + _boardPaddingPx;

        DrawBackground(drawList, cursorPos, new Vector2(0, headerHeightPx));

        _header.Draw(cursorPos, topRight.X - topLeft.X);
        
        cursorPos += new Vector2(0, headerHeightPx);

        _gameBoard.Draw(cursorPos);
        
        _footer.Draw(bottomLeft - _footerHeightPx);
    }

    private void DrawBackground(ImDrawListPtr drawList, Vector2 cursorPos, Vector2 headerHeightPx)
    {
        const uint backgroundColour = 0xFF808080; // ugly grey

        drawList.AddRectFilled(
            cursorPos - _boardPaddingPx,
            cursorPos + _gridSquareSizePx*_boardDimensions + _boardPaddingPx + headerHeightPx,
            backgroundColour);
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

    private bool MouseInSquare(Vector2 mousePos, Vector2 cursorPos, int squareSize)
        => mousePos.X > cursorPos.X
        && mousePos.X <= cursorPos.X + squareSize
        && mousePos.Y > cursorPos.Y
        && mousePos.Y <= cursorPos.Y + squareSize;
}
