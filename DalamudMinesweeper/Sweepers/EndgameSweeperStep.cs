using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;

public static class EndgameSweeperStep
{
    // if n mines, and n unflagged hidden tiles - all must be flagged
    public static bool Step(MinesweeperGame game)
    {
        if (game.GameState == GameState.Victorious)
            return false;

        var preState = SweeperGameState.From(game);

        if (game.NumUnflaggedMines() == NumUnflaggedHidden(game))
        {
            FlagAllUnflaggedHidden(game);
        }

        var postState = SweeperGameState.From(game);
        return preState != postState;
    }

    private static int NumUnflaggedHidden(MinesweeperGame game)
    {
        int numUnflaggedHidden = 0;
        for (int x = 0; x < game.Width; x++)
        {
            for (int y = 0; y < game.Height; y++)
            {
                var cell = game.GetCell(x, y);
                if (cell is { isFlagged: false, isRevealed: false })
                    numUnflaggedHidden++;
            }
        }
        return numUnflaggedHidden;
    }

    private static void FlagAllUnflaggedHidden(MinesweeperGame game)
    {
        for (int x = 0; x < game.Width; x++)
        {
            for (int y = 0; y < game.Height; y++)
            {
                var cell = game.GetCell(x, y);
                if (cell is { isFlagged: false, isRevealed: false })
                    game.RightClick(x, y);
            }
        }
    }
}
