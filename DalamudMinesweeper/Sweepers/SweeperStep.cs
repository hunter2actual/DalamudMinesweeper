using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;

public abstract class SweeperStep(MinesweeperGame game)
{
    public MinesweeperGame Game { get; set; } = game;
    public abstract void Step();
}