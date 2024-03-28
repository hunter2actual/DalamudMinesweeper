using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using DalamudMinesweeper.Game;
using ImGuiNET;

namespace DalamudMinesweeper.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private readonly ClassicSprites _classicSprites;
    private readonly Configuration _configuration;
    private MinesweeperGame _game;
    private int _gridSquareSize;
    private Vector2 _gridSquareSizeVec2;
    private Vector2 _boardDimensions;
    private readonly Vector2 _boardPadding;
    private readonly int _dalamudWindowPadding = 8;
    private readonly Vector2 footerHeight = new Vector2(0, 26);

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

        _plugin = plugin;
        _configuration = configuration;
        _classicSprites = new ClassicSprites(plugin.PluginInterface);
        _game = InitialiseGame();
        _gridSquareSizeVec2 = new Vector2(0, 0);
        _boardPadding = new Vector2(5, 5);
    }

    public void Dispose()
    {
        _classicSprites.Dispose();
    }

    public override void Draw()
    {
        _gridSquareSize = _classicSprites.Tile0.Width * _configuration.Zoom;
        _gridSquareSizeVec2.X = _gridSquareSizeVec2.Y = _gridSquareSize;
        _boardDimensions = new Vector2(_game.Width, _game.Height);
        var windowSize = new Vector2(
            _gridSquareSize*_boardDimensions.X + 2*_boardPadding.X + 2*_dalamudWindowPadding,
            _gridSquareSize*_boardDimensions.Y + 2*_boardPadding.Y + 2*_dalamudWindowPadding + 62);
        ImGui.SetWindowSize(windowSize);

        var windowPos = ImGui.GetWindowPos();

        // Get window corner coords (relative to window)
        // Content region is 8px padded from window
        var topLeft = ImGui.GetWindowContentRegionMin();
        var bottomRight = ImGui.GetWindowContentRegionMax();
        var topRight = new Vector2(bottomRight.X, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, bottomRight.Y);

        ImGui.InvisibleButton("anticlick", bottomRight - topLeft - footerHeight);
        
        var mousePos = ImGui.GetMousePos();
        var cursorPos = windowPos + topLeft + _boardPadding;

        var drawList = ImGui.GetWindowDrawList();

        DrawBackground(drawList, cursorPos);

        for (int y = 0; y < _boardDimensions.Y; y++) {
            for (int x = 0; x < _boardDimensions.X; x++) {
                drawList.AddImage(GetCellImage(_game.GetCell(x, y)).ImGuiHandle, cursorPos, cursorPos + _gridSquareSizeVec2);

                if (MouseInSquare(mousePos, cursorPos) && ImGui.IsWindowFocused()) {
                    DrawHighlightSquare(drawList, cursorPos);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && !_game.GetCell(x,y).isFlagged) {
                        _game.Click(x, y);
                    }
                    else if (ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                        _game.Flag(x, y);
                    }
                }

                cursorPos.X += _gridSquareSize;
            }
            cursorPos.Y += _gridSquareSize;
            cursorPos.X -= _boardDimensions.X * _gridSquareSize;
        }

        DrawFooter(bottomLeft - footerHeight, bottomRight.X - bottomLeft.X);
    }

    private void DrawBackground(ImDrawListPtr drawList, Vector2 cursorPos)
    {
        const uint backgroundColour = 0xFF808080; // ugly grey

        drawList.AddRectFilled(
            cursorPos - _boardPadding,
            cursorPos + _gridSquareSize*_boardDimensions + _boardPadding,
            backgroundColour);
    }

    private void DrawHighlightSquare(ImDrawListPtr drawList, Vector2 cursorPos)
    {
        const uint highlightSquareColour = 0x44FFFFFF; // translucent white

        drawList.AddRectFilled(
            cursorPos,
            cursorPos + _gridSquareSizeVec2,
            highlightSquareColour);
    }

    private void DrawFooter(Vector2 start, float footerWidth)
    {
        ImGui.SetCursorPos(start);

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
        {
            _plugin.DrawConfigUI();
        }        
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus)) {
            _configuration.Zoom++;
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Minus) && _configuration.Zoom > 1) {
            _configuration.Zoom--;
        };
        ImGui.SameLine();
        if (_game.GameState is GameState.Victorious)
            ImGui.Text("You Win!");
        else if (_game.GameState is GameState.Boom)
            ImGui.TextColored(ImGuiColors.DalamudRed, "YOU DIED");
        ImGui.SameLine();
        
        var rightOffset = ImGui.CalcTextSize("New Game").X + _dalamudWindowPadding;
        ImGui.SetCursorPosX(start.X + footerWidth - rightOffset);
        if (ImGui.Button("New Game"))
        {
            InitialiseGame();
        }
    }

    private IDalamudTextureWrap GetCellImage(Cell cell) {
        if (!cell.isRevealed) {
            if (cell.isFlagged) {
                return _classicSprites.TileFlag;
            }
            return _classicSprites.TileHidden;
        }
        return cell.contents switch {
            CellContents.Clear => _classicSprites.Tile0,
            CellContents.Mine => _classicSprites.TileMine,
            CellContents.ExplodedMine => _classicSprites.TileMineBoom,
            CellContents.Number => cell.numNeighbouringMines switch {
                1 => _classicSprites.Tile1,
                2 => _classicSprites.Tile2,
                3 => _classicSprites.Tile3,
                4 => _classicSprites.Tile4,
                5 => _classicSprites.Tile5,
                6 => _classicSprites.Tile6,
                7 => _classicSprites.Tile7,
                8 => _classicSprites.Tile8,
                _ => throw new("Invalid number of mines in cell " + cell.numNeighbouringMines)
            },
            _ => throw new("Unknown cell contents.")
        };
    }

    private MinesweeperGame InitialiseGame() {
        return _game = new MinesweeperGame(
            _configuration.BoardWidth,
            _configuration.BoardHeight,
            _configuration.NumMines);
    }

    private bool MouseInSquare(Vector2 mousePos, Vector2 cursorPos)
        => mousePos.X > cursorPos.X
        && mousePos.X <= cursorPos.X + _gridSquareSize
        && mousePos.Y > cursorPos.Y
        && mousePos.Y <= cursorPos.Y + _gridSquareSize;
}
