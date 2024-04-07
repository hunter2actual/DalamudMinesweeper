using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Solver;

public class StepSolver(MinesweeperGame game)
{
    public MinesweeperGame Game { get; set; } = game;
    private Board _board => Game.Board;

    public void Step()
    {
        PlaceObviousFlags();
        ClickAllRevealedNumbers();
    }

    private void PlaceObviousFlags()
    {
        for (int x = 0; x < _board.width; x++) {
            for (int y = 0; y < _board.height; y++) {
                var cell = _board.cells[x, y];
                if (IsRevealedNumber(cell) && NumNeighbouringHiddenPanels(x, y) == cell.numNeighbouringMines)
                {
                    FlagAdjacent(x, y);
                }
            }
        }
    }

    private void ClickAllRevealedNumbers()
    {
        for (int x = 0; x < _board.width; x++) {
            for (int y = 0; y < _board.height; y++) {
                var cell = _board.cells[x, y];
                if (IsRevealedNumber(cell))
                    Game.Click(x, y);
            }
        }
    }

    private bool IsRevealedNumber(Cell cell)
        => cell is { isRevealed: true, isFlagged: false, contents: CellContents.Number};
    
    private int NumNeighbouringHiddenPanels(int x, int y)
    {
        int count = 0;

        // Loop through a square around the current cell
        for (int x2 = x-1; x2 <= x+1; x2++) {
            for (int y2 = y-1; y2 <= y+1; y2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= _board.width || y2 >= _board.height)
                    continue;
                
                if (!_board.cells[x2, y2].isRevealed)
                    count++;
            }
        }

        return count;
    }

    private void FlagAdjacent(int x, int y)
    {
        // Loop through a square around the current cell
        for (int x2 = x-1; x2 <= x+1; x2++) {
            for (int y2 = y-1; y2 <= y+1; y2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= _board.width || y2 >= _board.height)
                    continue;
                
                var cell = _board.cells[x2, y2];
                if (cell is {isRevealed: false, isFlagged: false})
                    Game.RightClick(x2, y2);
            }
        }
    }
}