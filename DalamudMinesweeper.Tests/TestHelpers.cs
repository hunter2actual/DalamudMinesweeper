using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Tests;

public static class TestHelpers
{
    public static MinesweeperGame InitialiseGame(string[] boardStrings, int initialX, int initialY)
    {
        var numMines = boardStrings.Sum(s => s.Count(c => c == '*'));
        var game = new MinesweeperGame(boardStrings[0].Length, boardStrings.Length, numMines, () => { });
        game.Click(initialX, initialY);
        game.Board = BoardBuilder.FromStrings(boardStrings);
        game.Click(initialX, initialY);
        return game;
    }
}
