using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DalamudMinesweeper.Components;
using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sweepers;
using DalamudMinesweeper.Sprites;
using Dalamud.Bindings.ImGui;

namespace DalamudMinesweeper.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly TileSprites _tileSprites;

    private readonly Configuration _configuration;
    private MinesweeperGame _game;
    private Vector2 _boardDimensions;
    private bool _canSaveScore; // latching to solve debounce

    // Pixel sizes
    private readonly int _dalamudWindowPaddingPx = 8;
    private int _gridSquareSizePx;
    private readonly int _borderWidthPx = 12;
    private readonly Vector2 _borderWidthPxVec2 = new(12, 12);
    private readonly int _titleBarHeightPx = 26;
    private readonly Vector2 _footerHeightPxVec2 = new(0, 26);

    // Renderable components
    private GameBoard _gameBoard;
    private Header _header;
    private Footer _footer;
    private Background _background;

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
        _tileSprites = new TileSprites(Service.PluginInterface);
        var numberSprites = new NumberSprites(Service.PluginInterface);

        _game = InitialiseGame();
        _gameBoard = new GameBoard(_game, _tileSprites, _configuration);
        _header = new Header(_game, _tileSprites, numberSprites, _configuration, () => InitialiseGame());
        _footer = new Footer(_game, _configuration, plugin.DrawConfigUI, plugin.DrawScoresUI, new Sweeper());
        _background = new Background(_game, _configuration, _borderWidthPx);
    }

    public override void Draw()
    {
        // Calculate element sizes
        var windowPos = ImGui.GetWindowPos();
        var headerHeightPx = (int) (_tileSprites.SmileySize.Y + 8) * _configuration.Zoom;

        _gridSquareSizePx = (int) _tileSprites.TileSize.X * _configuration.Zoom;
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

        _gameBoard.Draw(cursorPos);
        
        _footer.Draw(bottomLeft - _footerHeightPxVec2);
    }

    public override void OnClose()
    {
        _game.Pause();
        base.OnClose();
    }

    public override void OnOpen()
    {
        _game.Resume();
        base.OnOpen();
    }

    private MinesweeperGame InitialiseGame()
    {
        _game = new MinesweeperGame(
            _configuration.BoardWidth,
            _configuration.BoardHeight,
            _configuration.NumMines,
            _configuration.NoGuess,
            RecordScore,
            _configuration.NoGuessTimeoutMs,
            _configuration.RevealShortcut,
            _configuration.FlagShortcut);

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
        if (_footer is not null) 
        {
            _footer.Game = _game;
        }

        _canSaveScore = true;

        return _game;
    }

    private void RecordScore()
    {
        var gameParameters = new GameParameters(_game.Height, _game.Width, _game.NumMines);
        var time = _game.ElapsedGameTimeMs;

        if (_canSaveScore)
        {
            _configuration.Scores.scores.Add((gameParameters, time));
            _configuration.Save();
            _canSaveScore = false;
        }
    }

    public void Dispose() { }
}
