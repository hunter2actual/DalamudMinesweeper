using System.Numerics;
using Dalamud.Interface.Internal;
using DalamudMinesweeper.Game;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class GameBoard
{
    private readonly ClassicSprites _classicSprites;
    private readonly Configuration _configuration;
    private MinesweeperGame _game;
    private Vector2 _gridSquareSizePxVec2 = new Vector2();

    public bool CellClickActive { get; private set;}

    public GameBoard(MinesweeperGame game, ClassicSprites classicSprites, Configuration configuration)
    {
        _game = game;
        _classicSprites = classicSprites;
        _configuration = configuration;
    }

    public void Draw(Vector2 start)
    {
        var cursorPos = start;
        var mousePos = ImGui.GetMousePos();
        var drawList = ImGui.GetWindowDrawList();

        var gridSquareSizePx = _classicSprites.Tile0.Width * _configuration.Zoom;
        _gridSquareSizePxVec2.X = _gridSquareSizePxVec2.Y = gridSquareSizePx;

        for (int y = 0; y < _game.Height; y++) {
            for (int x = 0; x < _game.Width; x++) {
                drawList.AddImage(GetCellImage(_game.GetCell(x, y)).ImGuiHandle, cursorPos, cursorPos + _gridSquareSizePxVec2);

                if (MouseInSquare(mousePos, cursorPos, gridSquareSizePx) && ImGui.IsWindowFocused()) {
                    DrawHighlightSquare(drawList, cursorPos);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && !_game.GetCell(x,y).isFlagged) {
                        _game.Click(x, y);
                        // TODO active click logic
                    }
                    else if (ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                        _game.Flag(x, y);
                    }
                }

                cursorPos.X += gridSquareSizePx;
            }
            cursorPos.Y += gridSquareSizePx;
            cursorPos.X -= _game.Width * gridSquareSizePx;
        }
    }

    private IDalamudTextureWrap GetCellImage(Cell cell)
    {
        if (!cell.isRevealed)
        {
            if (cell.isFlagged)
            {
                return _classicSprites.TileFlag;
            }
            return _classicSprites.TileHidden;
        }
        return cell.contents switch
        {
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

    
    private void DrawHighlightSquare(ImDrawListPtr drawList, Vector2 cursorPos)
    {
        const uint highlightSquareColour = 0x44FFFFFF; // translucent white

        drawList.AddRectFilled(
            cursorPos,
            cursorPos + _gridSquareSizePxVec2,
            highlightSquareColour);
    }

    private bool MouseInSquare(Vector2 mousePos, Vector2 cursorPos, int squareSize)
        => mousePos.X > cursorPos.X
        && mousePos.X <= cursorPos.X + squareSize
        && mousePos.Y > cursorPos.Y
        && mousePos.Y <= cursorPos.Y + squareSize;
}