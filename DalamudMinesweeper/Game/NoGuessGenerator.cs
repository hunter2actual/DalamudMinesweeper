using DalamudMinesweeper.Sweepers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DalamudMinesweeper.Game;

public class NoGuessGenerator
{
    private int _width { get; init; }
    private int _height { get; init; }
    private int _numMines { get; init; }
    private int _noGuessTimeoutMs { get; init; }
    private Sweeper _sweeper { get; }


    public NoGuessGenerator(int width, int height, int numMines, int noGuessTimeoutMs)
    {
        _width = width;
        _height = height;
        _numMines = numMines;
        _noGuessTimeoutMs = noGuessTimeoutMs;
        _sweeper = new Sweeper();
    }


    public Board Generate(int initialX, int initialY)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(_noGuessTimeoutMs);
        var ct = cts.Token;

        _sweeper.Timeout = TimeSpan.FromMilliseconds(_noGuessTimeoutMs);

        bool succeeded = false;
        Board resultBoard = new();

        while (!succeeded)
        {
            ct.ThrowIfCancellationRequested();
            List<Task<(bool swept, Board board)>> tasks = [];
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(TestBoard(initialX, initialY, ct));
            }
            Task.WaitAll(tasks.ToArray());
            if (tasks.Select(t => t.Result).Any(x => x.swept))
            {
                succeeded = true;
                resultBoard = tasks.Select(t => t.Result).First(x => x.swept).board;
            }
        }

        return resultBoard;
    }

    private MinesweeperGame DummyGame()
        => new MinesweeperGame(_width, _height, _numMines, false, () => { });

    private async Task<(bool swept, Board board)> TestBoard(int initialX, int initialY, CancellationToken ct)
    {
        var game = DummyGame();
        game.Click(initialX, initialY);
        var resultBoard = BoardExtensions.From(game.Board);

        await _sweeper.SweepAsync(game);
        return (_sweeper.Swept, resultBoard);
    }
}
