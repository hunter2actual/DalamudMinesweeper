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
    private int _gridSquareSizePx;
    private Vector2 _gridSquareSizePxVec2;
    private Vector2 _boardDimensions;
    private bool _smileyClicked;
    private readonly Vector2 _boardPaddingPx = new Vector2(5, 5);
    private readonly int _dalamudWindowPaddingPx = 8;
    private readonly int _titleBarHeightPx = 26;
    private readonly Vector2 _footerHeightPx = new Vector2(0, 26);

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
        _gridSquareSizePxVec2 = new Vector2(0, 0);
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

        ImGui.InvisibleButton("anticlick", bottomRight - topLeft - _footerHeightPx);
        
        var mousePos = ImGui.GetMousePos();
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = windowPos + topLeft + _boardPaddingPx ;

        DrawBackground(drawList, cursorPos, new Vector2(0, headerHeightPx));
        DrawHeader(drawList, cursorPos, topRight.X - topLeft.X, mousePos);
        cursorPos += new Vector2(0, headerHeightPx);

        for (int y = 0; y < _boardDimensions.Y; y++) {
            for (int x = 0; x < _boardDimensions.X; x++) {
                drawList.AddImage(GetCellImage(_game.GetCell(x, y)).ImGuiHandle, cursorPos, cursorPos + _gridSquareSizePxVec2);

                if (MouseInSquare(mousePos, cursorPos, _gridSquareSizePx) && ImGui.IsWindowFocused()) {
                    DrawHighlightSquare(drawList, cursorPos);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && !_game.GetCell(x,y).isFlagged) {
                        _game.Click(x, y);
                    }
                    else if (ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                        _game.Flag(x, y);
                    }
                }

                cursorPos.X += _gridSquareSizePx;
            }
            cursorPos.Y += _gridSquareSizePx;
            cursorPos.X -= _boardDimensions.X * _gridSquareSizePx;
        }

        DrawFooter(bottomLeft - _footerHeightPx, bottomRight.X - bottomLeft.X);
    }

    private void DrawBackground(ImDrawListPtr drawList, Vector2 cursorPos, Vector2 headerHeightPx)
    {
        const uint backgroundColour = 0xFF808080; // ugly grey

        drawList.AddRectFilled(
            cursorPos - _boardPaddingPx,
            cursorPos + _gridSquareSizePx*_boardDimensions + _boardPaddingPx + headerHeightPx,
            backgroundColour);
    }

    private void DrawHighlightSquare(ImDrawListPtr drawList, Vector2 cursorPos)
    {
        const uint highlightSquareColour = 0x44FFFFFF; // translucent white

        drawList.AddRectFilled(
            cursorPos,
            cursorPos + _gridSquareSizePxVec2,
            highlightSquareColour);
    }

    private void DrawHeader(ImDrawListPtr drawList, Vector2 start, float headerWidth, Vector2 mousePos)
    {
        IDalamudTextureWrap smileyToDraw = _classicSprites.Smiley;
        var smileySize = smileyToDraw.Size * _configuration.Zoom;
        float leftPadding = (float) ((headerWidth - smileySize.X) * 0.5);
        var cursorPos = start + new Vector2(leftPadding, 0);

        if (_game.GameState == GameState.Victorious)
        {
            smileyToDraw = _classicSprites.SmileyShades;
        }
        else if (_game.GameState == GameState.Boom)
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
                InitialiseGame();
            }
        }
        
        // TODO soyface when clicking on game

        drawList.AddImage(smileyToDraw.ImGuiHandle, cursorPos, cursorPos + smileySize);
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
        
        var rightOffset = ImGui.CalcTextSize("New Game").X + _dalamudWindowPaddingPx;
        ImGui.SetCursorPosX(start.X + footerWidth - rightOffset);
        if (ImGui.Button("New Game"))
        {
            InitialiseGame();
        }
    }

    private IDalamudTextureWrap GetCellImage(Cell cell)
    {
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

    private MinesweeperGame InitialiseGame()
    {
        return _game = new MinesweeperGame(
            _configuration.BoardWidth,
            _configuration.BoardHeight,
            _configuration.NumMines);
    }

    private bool MouseInSquare(Vector2 mousePos, Vector2 cursorPos, int squareSize)
        => mousePos.X > cursorPos.X
        && mousePos.X <= cursorPos.X + squareSize
        && mousePos.Y > cursorPos.Y
        && mousePos.Y <= cursorPos.Y + squareSize;
}
