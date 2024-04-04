using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DalamudMinesweeper.Components;
using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sprites;
using ImGuiNET;

namespace DalamudMinesweeper.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly TileSprites _tileSprites;
    private readonly NumberSprites _numberSprites;

    private readonly Configuration _configuration;
    private MinesweeperGame _game;
    private Vector2 _boardDimensions;

    // Pixel sizes
    private readonly int _dalamudWindowPaddingPx = 8;
    private int _gridSquareSizePx;
    private Vector2 _gridSquareSizePxVec2;
    private readonly int _borderWidthPx = 12;
    private readonly Vector2 _borderWidthPxVec2 = new(12, 12);
    private readonly int _titleBarHeightPx = 26;
    private readonly Vector2 _footerHeightPxVec2 = new(0, 26);

    // Renderable components
    private GameBoard _gameBoard;
    private Header _header;
    private Footer _footer;
    private Background _background;
    private bool _drawBoard;

    public MainWindow(Plugin plugin, Configuration configuration) : base("Minesweeper",
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
        _tileSprites = new TileSprites(plugin.PluginInterface);
        _numberSprites = new NumberSprites(plugin.PluginInterface);
        _gridSquareSizePxVec2 = new Vector2(0, 0);

        _game = InitialiseGame();
        _gameBoard = new GameBoard(_game, _tileSprites, _configuration);
        _header = new Header(_game, _tileSprites, _numberSprites, _configuration, () => InitialiseGame());
        _footer = new Footer(_configuration, plugin.DrawConfigUI);
        _background = new Background(_game, _configuration, _borderWidthPx);
        _drawBoard = true;
    }

    public void Dispose()
    {
        _tileSprites.Dispose();
        _numberSprites.Dispose();
    }

    public override void Draw()
    {
        // Calculate element sizes
        var windowPos = ImGui.GetWindowPos();
        var headerHeightPx = (int) (_tileSprites.SmileySize.Y + 8)* _configuration.Zoom;

        _gridSquareSizePx = (int) _tileSprites.TileSize.X * _configuration.Zoom;
        _gridSquareSizePxVec2.X = _gridSquareSizePxVec2.Y = _gridSquareSizePx;
        _boardDimensions = new Vector2(_game.Width, _game.Height);
        var windowWidthPx = _gridSquareSizePx*_boardDimensions.X + 2*_borderWidthPx*_configuration.Zoom + 2*_dalamudWindowPaddingPx;
        var windowHeightPx = _gridSquareSizePx*_boardDimensions.Y + 2*_borderWidthPx*_configuration.Zoom + 2*_dalamudWindowPaddingPx
                            + _footerHeightPxVec2.Y + headerHeightPx + _borderWidthPx*_configuration.Zoom + _titleBarHeightPx + 6; // this shit feels like I'm doing CSS 🤮
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
        var cursorPos = windowPos + topLeft;

        _background.Draw(cursorPos, Vector2.UnitY*headerHeightPx, _gridSquareSizePx);

        cursorPos += _borderWidthPxVec2 * _configuration.Zoom;

        var headerWidth = (int) (topRight.X - topLeft.X - (2*_borderWidthPx - 1)*_configuration.Zoom);
        _header.Draw(cursorPos, headerWidth, headerHeightPx);
        
        cursorPos += Vector2.UnitY*(headerHeightPx + (_borderWidthPx-1)*_configuration.Zoom);

        if (_drawBoard)
            _gameBoard.Draw(cursorPos);
        
        _footer.Draw(bottomLeft - _footerHeightPxVec2);

        if (ImGui.Checkbox("Draw board", ref _drawBoard)) {}
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
        if (_background is not null) 
        {
            _background.Game = _game;
        }

        return _game;
    }
}
