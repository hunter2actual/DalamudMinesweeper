using System;
using System.Diagnostics;
using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;

public class Sweeper
{
    public Sweeper()
    {
        Stopwatch = new Stopwatch();
        NumSteps = 0;
    }

    public Stopwatch Stopwatch { get; init; }
    public int NumSteps { get; private set; }
    private readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    public bool Solve(MinesweeperGame game)
    {
        Stopwatch.StartNew();
        NumSteps = 0;
        while (!SimpleSweeperStep.Step(game) && Stopwatch.Elapsed < Timeout)
        {
            NumSteps++;
        }
        Stopwatch.Stop();
        return game.GameState == GameState.Victorious;
    }
}
