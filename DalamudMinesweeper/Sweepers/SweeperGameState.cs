using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;

public record SweeperGameState(int NumHiddenPanels, int NumFlags, GameState GameState)
{
    public static SweeperGameState From(MinesweeperGame game)
    {
        int hidden = 0, flags = 0;

        for (int x = 0; x < game.Board.width; x++) {
            for (int y = 0; y < game.Board.height; y++) {
                var cell = game.Board.cells[x, y];
                if (cell.isFlagged)
                {
                    flags++;
                }
                else if (!cell.isRevealed)
                {
                    hidden++;
                }
            }
        }

        return new SweeperGameState(hidden, flags, game.GameState);
    }
}