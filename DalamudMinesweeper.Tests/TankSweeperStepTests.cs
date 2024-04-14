using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sweepers;

namespace DalamudMinesweeper.Tests;

public class TankSweeperStepTests
{
    [Fact]
    public void TrivialTankTest()
    {
        var board = new string[]
        {
            "     ",
            "     ",
            "     ",
            "   11",
            "   1*"
        };

        var game = TestHelpers.InitialiseGame(board, 0, 0);

        Assert.Equal(GameState.Playing, game.GameState);
        Assert.True(TankSweeperStep.Step(game)); // first step solves
        Assert.Equal(GameState.Victorious, game.GameState);
        Assert.False(SimpleSweeperStep.Step(game)); // second step no effect
    }

    [Fact]
    public void WideBoardTest()
    {
        var board = new string[]
        {
            "     ",
            "   11",
            "   1*"
        };

        var game = TestHelpers.InitialiseGame(board, 0, 0);

        Assert.Equal(GameState.Playing, game.GameState);
        Assert.True(TankSweeperStep.Step(game)); // first step solves
        Assert.Equal(GameState.Victorious, game.GameState);
        Assert.False(SimpleSweeperStep.Step(game)); // second step no effect
    }

    [Fact]
    public void TwoMineTankTest()
    {
        var board = new string[]
        {
            "   1*",
            "   11",
            "     ",
            "   11",
            "   1*"
        };

        var game = TestHelpers.InitialiseGame(board, 0, 0);

        Assert.Equal(GameState.Playing, game.GameState);
        Assert.True(TankSweeperStep.Step(game)); // first step solves
        Assert.Equal(GameState.Victorious, game.GameState);
        Assert.False(SimpleSweeperStep.Step(game)); // second step no effect
    }

    [Fact]
    public void TwoConnectedMinesTankTest()
    {
        var board = new string[]
        {
            "     ",
            "     ",
            "   11",
            "   2*",
            "   2*"
        };

        var game = TestHelpers.InitialiseGame(board, 0, 0);

        Assert.Equal(GameState.Playing, game.GameState);
        Assert.True(TankSweeperStep.Step(game)); // first step solves
        Assert.Equal(GameState.Victorious, game.GameState);
        Assert.False(SimpleSweeperStep.Step(game)); // second step no effect
    }

    [Fact]
    public void NonTrivialTankTest()
    {
        var board = new string[]
        {
            "         ",
            "         ",
            "         ",
            " 111     ",
            " 1*1     ",
            "1211   11",
            "*1     2*",
            " 2     3*",
            "*1     2*"
        };

        var game = TestHelpers.InitialiseGame(board, 0, 0);

        Assert.Equal(GameState.Playing, game.GameState);
        Assert.True(TankSweeperStep.Step(game)); // first step solves
        Assert.Equal(GameState.Victorious, game.GameState);
        Assert.False(SimpleSweeperStep.Step(game)); // second step no effect
    }

    //[Fact]
    //public void FlagCombinationsTest()
    //{
    //    var combinations = TankSweeperStep.FlagCombinations(3);

    //    Assert.Equal(8, combinations.Count);
    //}

    //[Fact]
    //public void NumFlagsTest()
    //{
    //    var board = new string[]
    //    {
    //        "1*1  ",
    //        "111  ",
    //        "   11",
    //        "   2*",
    //        "   2*"
    //    };

    //    var game = TestHelpers.InitialiseGame(board, 0, 4);

    //    var numFlags = TankSweeperStep.GetNumFlags(game);

    //    Assert.Equal(0, numFlags);
    //}

    //[Fact]
    //public void RelevantCellsTest()
    //{
    //    var board = new string[]
    //    {
    //        "1*1  ",
    //        "111  ",
    //        "   11",
    //        "   2*",
    //        "   2*"
    //    };

    //    var game = TestHelpers.InitialiseGame(board, 0, 4);

    //    var (numbersBorderingHiddens, hiddensBorderingNumbers) = TankSweeperStep.FindRelevantCells(game);

    //    Assert.Equal(4, hiddensBorderingNumbers.Count);
    //}
}