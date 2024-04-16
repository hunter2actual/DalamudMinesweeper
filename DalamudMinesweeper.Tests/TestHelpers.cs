using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Tests;

public static class TestHelpers
{
    public static MinesweeperGame InitialiseGame(string[] boardStrings, int initialX, int initialY)
    {
        var numMines = boardStrings.Sum(s => s.Count(c => c == '*'));
        var game = new MinesweeperGame(boardStrings[0].Length, boardStrings.Length, numMines, false, () => { }, revealShortcut: true);
        game.Click(initialX, initialY);
        game.Board = BoardBuilder.FromStrings(boardStrings);
        game.Click(initialX, initialY);
        return game;
    }

    public enum Difficulty { Easy, Medium, Hard }
    public static MinesweeperGame InitialiseGame(Difficulty difficulty)
    {
        int width, height, numMines;
        switch (difficulty)
        {
            case Difficulty.Hard:
                width = height = 24;
                numMines = 99;
                break;
            case Difficulty.Medium:
                width = height = 16;
                numMines = 40;
                break;
            default:
            case Difficulty.Easy:
                width = height = 9;
                numMines = 10;
                break;
        }

        var game = new MinesweeperGame(width, height, numMines, false, () => { });
        game.Click(3, 3);
        return game;
    }
}
