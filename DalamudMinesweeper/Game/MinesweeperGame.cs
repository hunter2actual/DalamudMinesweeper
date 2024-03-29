namespace DalamudMinesweeper.Game;

public class MinesweeperGame {

    public int Width { get; init; }
    public int Height { get; init; }
    private BoardBuilder _boardBuilder;
    private Board _board;
    public GameState GameState { get; private set; }
    private bool _firstMoveTaken;

    public MinesweeperGame(int width, int height, int numMines)
    {
        Width = width;
        Height = height;
        _boardBuilder = new BoardBuilder(width, height, numMines);
        _board = _boardBuilder.Build();
        GameState = GameState.Playing;
    }

    public void Click(int x, int y)
    {
        if (GameState != GameState.Playing) return;

        if (!_firstMoveTaken) {
            _boardBuilder.WithClearPosition(x, y);
            _board = _boardBuilder.Build();
            _firstMoveTaken = true;
        }

        var contents = _board.GetCellContents(x, y);
        var cell = GetCell(x, y);
        switch (contents) {
            case CellContents.Mine:
                _board.RevealCell(x, y);
                GameState = GameState.Boom;
                _board.SetCellContents(x, y, CellContents.ExplodedMine);
                RevealAll();
                break;
            case CellContents.Number:
                if (cell.isRevealed && _board.CellIsFulfilled(x, y))
                {
                    _board.ClickAdjacentUnflaggedTiles(x, y, Click);
                }
                else if (!cell.isRevealed)
                {
                    _board.RevealCell(x, y);
                }
                break;
            case CellContents.Clear:
                _board.RevealCell(x, y);
                _board.RevealConnectedField(x, y);
                break;
        }

        if(GameState is not GameState.Boom && _board.IsVictory())
            GameState = GameState.Victorious;
    }

    public void RightClick(int x, int y)
    {
        if (GameState != GameState.Playing || !_firstMoveTaken) return;

        _board.ToggleCellFlag(x, y);

        if(_board.IsVictory())
            GameState = GameState.Victorious;
    }

    public void RevealAll() => _board.RevealAll();

    public void HideAll() => _board.HideAll();

    public Cell GetCell(int x, int y) => _board.cells[x, y];
}