using System.Numerics;
using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sprites;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class GameBoard
{
    public MinesweeperGame Game { get; set; }
    private readonly TileSprites _tileSprites;
    private readonly Configuration _configuration;
    private Vector2 _gridSquareSizePxVec2 = new Vector2();

    public bool CellClickActive { get; private set;}

    public GameBoard(MinesweeperGame game, TileSprites tileSprites, Configuration configuration)
    {
        Game = game;
        _tileSprites = tileSprites;
        _configuration = configuration;
    }

    public void Draw(Vector2 start)
    {
        var cursorPos = start;
        var mousePos = ImGui.GetMousePos();
        var drawList = ImGui.GetWindowDrawList();

        var gridSquareSizePx = (int) _tileSprites.TileSize.X * _configuration.Zoom;
        _gridSquareSizePxVec2.X = _gridSquareSizePxVec2.Y = gridSquareSizePx;

        for (int y = 0; y < Game.Height; y++) {
            for (int x = 0; x < Game.Width; x++) {
                _tileSprites.DrawTile(drawList, Game.GetCell(x, y), cursorPos, _configuration.Zoom);

                if (MouseInSquare(mousePos, cursorPos, gridSquareSizePx) && ImGui.IsWindowFocused()) {
                    DrawHighlightSquare(drawList, cursorPos);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && !Game.GetCell(x,y).isFlagged) {
                        Game.Click(x, y);
                        // TODO active click logic
                    }
                    else if (ImGui.IsMouseReleased(ImGuiMouseButton.Right)) {
                        Game.RightClick(x, y);
                    }
                }

                cursorPos.X += gridSquareSizePx;
            }
            cursorPos.Y += gridSquareSizePx;
            cursorPos.X -= Game.Width * gridSquareSizePx;
        }
    }

    private void DrawHighlightSquare(ImDrawListPtr drawList, Vector2 cursorPos)
        => drawList.AddRectFilled(
            cursorPos,
            cursorPos + _gridSquareSizePxVec2,
            Colours.Highlight);

    private bool MouseInSquare(Vector2 mousePos, Vector2 cursorPos, int squareSize)
        => mousePos.X > cursorPos.X
        && mousePos.X <= cursorPos.X + squareSize
        && mousePos.Y > cursorPos.Y
        && mousePos.Y <= cursorPos.Y + squareSize;
}