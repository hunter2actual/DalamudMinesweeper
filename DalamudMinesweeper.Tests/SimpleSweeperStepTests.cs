using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sweepers;

namespace DalamudMinesweeper.Tests;

public class SimpleSweeperStepTests
{
    [Fact]
    public void SimpleTest()
    {
        var board = new string[]
        {
            "1*1  ",
            "111  ",
            "   11",
            "   2*",
            "   2*"
        };

        var game = TestHelpers.InitialiseGame(board, 0, 4);

        Assert.Equal(GameState.Playing, game.GameState);
        Assert.True(SimpleSweeperStep.Step(game)); // first step solves
        Assert.Equal(GameState.Victorious, game.GameState);
        Assert.False(SimpleSweeperStep.Step(game)); // second step no effect
    }
}