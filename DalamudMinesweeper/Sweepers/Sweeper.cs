using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
    public bool Swept { get; private set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

    public async Task SweepAsync(MinesweeperGame game)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(Timeout);
        Stopwatch.Restart();
        NumSimpleSteps = NumTankSteps = 0;
        Swept = false;

        try
        {
            while(Simple(game) || await TankAsync(game, cts.Token))
            {
                // no-op
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            Stopwatch.Stop();
            Swept = game.GameState == GameState.Victorious;
        }
    }

    private bool Simple(MinesweeperGame game)
    {
        NumSimpleSteps++;
        return SimpleSweeperStep.Step(game);
    }

    private Task<bool> TankAsync(MinesweeperGame game, CancellationToken ct)
    {
        NumTankSteps++;
        return TankSweeperStep.StepAsync(game, ct);
    }
}
