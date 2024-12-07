using System;
using System.Diagnostics;
using System.Linq;

namespace DalamudMinesweeper.Game;

public class MinesweeperGame
{
    public int Width { get; init; }
    public int Height { get; init; }
    public int NumMines { get; init; }
    public bool NoGuessValid { get; private set; }
    private BoardBuilder _boardBuilder;
    private NoGuessGenerator _noGuessGenerator;
    public Board Board;
    public GameState GameState { get; private set; }
    private bool _firstMoveTaken;
    private Stopwatch _stopwatch;
    private readonly bool _noGuess;
    private Action _onVictory;
    private readonly bool _revealShortcut;
    private readonly bool _flagShortcut;


    public MinesweeperGame(int width, int height, int numMines, bool noGuess, Action onVictory,
                           int noGuessTimeoutMs = 1500, bool revealShortcut = false, bool flagShortcut = false)
    {
        Width = width;
        Height = height;
        NumMines = numMines;
        _boardBuilder = new BoardBuilder(width, height, numMines);
        Board = _boardBuilder.Build();
        GameState = GameState.Playing;
        _stopwatch = new Stopwatch();
        _noGuess = noGuess;
        _noGuessGenerator = new NoGuessGenerator(Width, Height, NumMines, noGuessTimeoutMs);
        NoGuessValid = true;
        _onVictory = onVictory;
        _revealShortcut = revealShortcut;
        _flagShortcut = flagShortcut;
    }

    public void Click(int x, int y)
    {
        if (GameState != GameState.Playing) return;

        if (!_firstMoveTaken)
        {
            _boardBuilder.WithClearPosition(x, y);
            try
            {
                Board = _noGuess ? _noGuessGenerator.Generate(x, y) : _boardBuilder.Build();
                NoGuessValid = true;
            }
            catch
            {
                Board = _boardBuilder.Build();
                NoGuessValid = false;
            }
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
                if (_revealShortcut && cell.isRevealed && Board.CellIsFulfilledForRevealing(x, y))
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

        if (_flagShortcut)
        {
            var cell = GetCell(x, y);
            if (cell.contents is CellContents.Number && Board.CellIsFulfilledForFlagging(x, y))
                Board.FlagAdjacentHiddenTiles(x, y, RightClick);
        }

        Board.ToggleCellFlag(x, y);

        if(Board.IsVictory()) Win();
    }

    public void Win()
    {
        GameState = GameState.Victorious;
        FlagAllMines();
        _onVictory();
        _stopwatch.Stop();
    }

    public void RevealAll() => Board.RevealAll();

    public void HideAll() => Board.HideAll();

    public void FlagAllMines() => Board.FlagAllMines();

    public Cell GetCell(int x, int y) => Board.cells[x, y];

    public int NumUnflaggedMines()
    {
        var cells = Board.cells.ToList();

        return cells.Count(c => c.contents is CellContents.Mine or CellContents.ExplodedMine)
             - cells.Count(c => c.isFlagged);
    }

    public int ElapsedGameTime => (int) _stopwatch.Elapsed.TotalSeconds;
    public long ElapsedGameTimeMs => _stopwatch.ElapsedMilliseconds;
    public void Pause()
    {
        if (_firstMoveTaken)
            _stopwatch.Stop();
    }

    public void Resume()
    {
        if (_firstMoveTaken)
            _stopwatch.Start();
    }
}