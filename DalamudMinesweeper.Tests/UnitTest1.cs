using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sweepers;

namespace DalamudMinesweeper.Tests;

public class SimpleSweeperStepTests
{
    [Fact]
    public void Test1()
    {
        var game = new MinesweeperGame(9, 9, 1, () => {});
        game.Click(2, 2);

        Assert.True(SimpleSweeperStep.Step(game));
    }
}