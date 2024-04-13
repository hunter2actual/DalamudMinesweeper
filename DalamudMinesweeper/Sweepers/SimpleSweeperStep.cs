using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;

public static class SimpleSweeperStep
{
    /*
     * 1) If a number n has n adjacent hidden tiles, they must all be mines, so flag them
     * 2) If a number n has n adjacent flags, every other adjace must be safe, so click them
     *    We leverage the Game.Click() logic for 2)
     */
    public static bool Step(MinesweeperGame game)
    {
        var preState = SweeperGameState.From(game);
        PlaceObviousFlags(game);
        ClickAllRevealedNumbers(game);
        var postState = SweeperGameState.From(game);
        return preState == postState;
    }

    private static void PlaceObviousFlags(MinesweeperGame game)
    {
        for (int x = 0; x < game.Width; x++) {
            for (int y = 0; y < game.Height; y++) {
                var cell = game.Board.cells[x, y];
                if (IsRevealedNumber(cell) && NumNeighbouringHiddenPanels(game, x, y) == cell.numNeighbouringMines)
                {
                    FlagAdjacent(game, x, y);
                }
            }
        }
    }

    private static void ClickAllRevealedNumbers(MinesweeperGame game)
    {
        for (int x = 0; x < game.Width; x++) {
            for (int y = 0; y < game.Height; y++) {
                var cell = game.Board.cells[x, y];
                if (IsRevealedNumber(cell))
                    game.Click(x, y);
            }
        }
    }

    private static bool IsRevealedNumber(Cell cell)
        => cell is { isRevealed: true, isFlagged: false, contents: CellContents.Number};
    
    private static int NumNeighbouringHiddenPanels(MinesweeperGame game, int x, int y)
    {
        int count = 0;

        // Loop through a square around the current cell
        for (int x2 = x-1; x2 <= x+1; x2++) {
            for (int y2 = y-1; y2 <= y+1; y2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= game.Width || y2 >= game.Height)
                    continue;
                
                if (!game.Board.cells[x2, y2].isRevealed)
                    count++;
            }
        }

        return count;
    }

    private static void FlagAdjacent(MinesweeperGame game, int x, int y)
    {
        // Loop through a square around the current cell
        for (int x2 = x-1; x2 <= x+1; x2++) {
            for (int y2 = y-1; y2 <= y+1; y2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= game.Width || y2 >= game.Height)
                    continue;
                
                var cell = game.Board.cells[x2, y2];
                if (cell is {isRevealed: false, isFlagged: false})
                    game.RightClick(x2, y2);
            }
        }
    }
}
