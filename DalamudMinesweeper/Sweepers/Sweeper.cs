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
        NumSimpleSteps = 0;
        NumTankSteps = 0;
    }

    public Stopwatch Stopwatch { get; init; }
    public int NumSimpleSteps { get; private set; }
    public int NumTankSteps { get; private set; }
    private readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    public bool Sweep(MinesweeperGame game)
    {
        Stopwatch.Restart();
        NumSimpleSteps = NumTankSteps = 0;
        do
        {
            do
            {
                NumSimpleSteps++;
            } while (SimpleSweeperStep.Step(game) && Stopwatch.Elapsed < Timeout);
            NumTankSteps++;
        } while (TankSweeperStep.Step(game) && Stopwatch.Elapsed < Timeout);
        Stopwatch.Stop();
        return game.GameState == GameState.Victorious;
    }

    public bool SimpleSweep(MinesweeperGame game)
    {
        Stopwatch.Restart();
        NumSimpleSteps = 0;
        do
        {
            NumSimpleSteps++;
        } while (SimpleSweeperStep.Step(game) && Stopwatch.Elapsed < Timeout);
        Stopwatch.Stop();
        return game.GameState == GameState.Victorious;
    }

    public bool TankSweep(MinesweeperGame game)
    {
        Stopwatch.Restart();
        NumTankSteps = 0;
        do 
        {
            NumTankSteps++;
        } while (TankSweeperStep.Step(game) && Stopwatch.Elapsed < Timeout);
        Stopwatch.Stop();
        return game.GameState == GameState.Victorious;
    }
}
