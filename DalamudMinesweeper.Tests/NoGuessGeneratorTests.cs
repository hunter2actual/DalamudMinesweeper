using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sweepers;

namespace DalamudMinesweeper.Tests;

public class NoGuessGeneratorTests
{
    //[Fact]
    //public async Task CanGenerateBoard()
    //{
    //    var boardBuilder = new BoardBuilder(8, 8, 5);
    //    boardBuilder.WithClearPosition(3, 3);
    //    var noGuessGenerator = new NoGuessGenerator(8, 8, 5, 3000);

    //    var res = noGuessGenerator.Generate(3, 3);

    //    Assert.False(res.IsVictory());

    //    var game = new MinesweeperGame(8, 8, 5, false, () => { });
    //    game.Board = res;
    //    game.Click(3, 3);

    //    var sweeper = new Sweeper();
    //    await sweeper.SweepAsync(game);
    //    Assert.True(sweeper.Swept);
    //}
}
