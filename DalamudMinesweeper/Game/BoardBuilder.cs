using System;

namespace DalamudMinesweeper.Game;

public class BoardBuilder {
    private readonly int _height;
    private readonly int _width;
    private readonly int _numMines;
    private int? _yStart;
    private int? _xStart;

    public BoardBuilder(int width, int height, int numMines, int? xStart = null, int? yStart = null)
    {
        _width = width;
        _height = height;
        _numMines = numMines;
        _xStart = xStart;
        _yStart = yStart;

        if (numMines > (width * height) - 9)
            throw new("Too many mines!");
    }

    public Board Build()
    {
        var board = new Board {
            width = _width,
            height = _height,
            cells = new Cell[_width, _height]
        };

        board = PopulateCells(board);
        board = PlaceMines(board);
        board = SetNeighbouringMines(board);
        return board;
    }

    // It's a test helper, pray forgive me
    public static Board FromStrings(string[] boardStrings)
    {
        var width = boardStrings[0].Length;
        var height = boardStrings.Length;
        var board = new Board
        {
            width = width,
            height = height,
            cells = new Cell[width, height]
        };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var inputChar = boardStrings[y][x];

                board.cells[x, y] = new Cell
                {
                    isRevealed = false,
                    contents = inputChar switch
                    {
                        '*' => CellContents.Mine,
                        ' ' => CellContents.Clear,
                        _ => CellContents.Number
                    }
                };
            }
        }

        board = SetNeighbouringMines(board);

        return board;
    }

    public BoardBuilder WithClearPosition(int x, int y)
    {
        _xStart = x;
        _yStart = y;
        return this;
    }

    private Board PopulateCells(Board board)
    {
        for (int x = 0; x < board.width; x++) {
            for (int y = 0; y < board.height; y++) {
                board.cells[x, y] = new Cell {
                    isRevealed = false,
                    contents = CellContents.Clear
                };
            }
        }
        return board;
    }

    private Board PlaceMines(Board board)
    {
        var random = new Random();
        var placedMines = 0;
        Cell currentCell;
        
        while (placedMines < _numMines) {
            var x = random.Next(0, board.width);
            var y = random.Next(0, board.height);

            // Guarantee starting position and surrounding squares are free
            if (_xStart is not null && _yStart is not null 
                && Math.Abs((int)_xStart - x) <= 1 && Math.Abs((int)_yStart - y) <= 1) {
                continue;
            }

            currentCell = board.cells[x, y];
            if (currentCell.contents == CellContents.Mine)
                continue;
            currentCell.contents = CellContents.Mine;
            placedMines++;
        }
        return board;
    }

    record point2(int x, int y);

    private static Board SetNeighbouringMines(Board board)
    {
        Cell currentCell;
        for (int x = 0; x < board.width; x++) {
            for (int y = 0; y < board.height; y++) {
                currentCell = board.cells[x, y];
                currentCell.numNeighbouringMines = 0;

                // Loop through a square around the current cell
                for (int x2 = x-1; x2 <= x+1; x2++) {
                    for (int y2 = y-1; y2 <= y+1; y2++) {
                        // Skip self
                        if (x2 == x && y2 == y)
                            continue;
                        
                        // Avoid out of bounds
                        if (x2 < 0 || y2 < 0 || x2 >= board.width || y2 >= board.height)
                            continue;
                        
                        if (board.cells[x2, y2].contents == CellContents.Mine)
                            currentCell.numNeighbouringMines++;
                    }
                }

                if (currentCell.numNeighbouringMines > 0 && currentCell.contents != CellContents.Mine) {
                    currentCell.contents = CellContents.Number;
                }
            }
        }
        return board;
    }
}