using System;
using System.Diagnostics;
using System.Threading;
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
    private readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    public bool SimpleSweep(MinesweeperGame game)
    {
        Stopwatch.Restart();
        NumSteps = 0;
        do
        {
            NumSteps++;
        } while (SimpleSweeperStep.Step(game) && Stopwatch.Elapsed < Timeout);
        Stopwatch.Stop();
        return game.GameState == GameState.Victorious;
    }

    public bool TankSweep(MinesweeperGame game)
    {
        Stopwatch.Restart();
        NumSteps = 0;
        do 
        {
            NumSteps++;
        } while (TankSweeperStep.Step(game) && Stopwatch.Elapsed < Timeout);
        Stopwatch.Stop();
        return game.GameState == GameState.Victorious;
    }
}
