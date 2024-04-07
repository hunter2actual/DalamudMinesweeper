using System;
using System.Diagnostics;
using System.Linq;

namespace DalamudMinesweeper.Game;

public class MinesweeperGame {

    public int Width { get; init; }
    public int Height { get; init; }
    public int NumMines { get; init; }
    private BoardBuilder _boardBuilder;
    public Board Board;
    public GameState GameState { get; private set; }
    private bool _firstMoveTaken;
    private Stopwatch _stopwatch;
    private Action _onVictory;

    public MinesweeperGame(int width, int height, int numMines, Action onVictory)
    {
        Width = width;
        Height = height;
        NumMines = numMines;
        _boardBuilder = new BoardBuilder(width, height, numMines);
        Board = _boardBuilder.Build();
        GameState = GameState.Playing;
        _stopwatch = new Stopwatch();
        _onVictory = onVictory;
    }

    public void Click(int x, int y)
    {
        if (GameState != GameState.Playing) return;

        if (!_firstMoveTaken) {
            _boardBuilder.WithClearPosition(x, y);
            Board = _boardBuilder.Build();
            _firstMoveTaken = true;
            _stopwatch.Start();
        }

        var contents = Board.GetCellContents(x, y);
        var cell = GetCell(x, y);
        switch (contents) {
            case CellContents.Mine:
                Board.RevealCell(x, y);
                GameState = GameState.Boom;
                Board.SetCellContents(x, y, CellContents.ExplodedMine);
                _stopwatch.Stop();
                RevealAll();
                break;
            case CellContents.Number:
                if (cell.isRevealed && Board.CellIsFulfilled(x, y))
                {
                    Board.ClickAdjacentUnflaggedTiles(x, y, Click);
                }
                else if (!cell.isRevealed)
                {
                    Board.RevealCell(x, y);
                }
                break;
            case CellContents.Clear:
                Board.RevealCell(x, y);
                Board.RevealConnectedField(x, y);
                break;
        }

        if(GameState is not GameState.Boom && Board.IsVictory()) Win();
    }

    public void RightClick(int x, int y)
    {
        if (GameState != GameState.Playing || !_firstMoveTaken) return;

        Board.ToggleCellFlag(x, y);

        if(Board.IsVictory()) Win();
    }

    public void Win()
    {
        GameState = GameState.Victorious;
        _onVictory();
        _stopwatch.Stop();
    }

    public void RevealAll() => Board.RevealAll();

    public void HideAll() => Board.HideAll();

    public Cell GetCell(int x, int y) => Board.cells[x, y];

    public int NumUnflaggedMines()
    {
        var cells = Board.cells.ToList();

        return cells.Count(c => c.contents is CellContents.Mine or CellContents.ExplodedMine)
             - cells.Count(c => c.isFlagged);
    }

    public int ElapsedGameTime => (int) _stopwatch.Elapsed.TotalSeconds;
    public long ElapsedGameTimeMs => _stopwatch.ElapsedMilliseconds;
}