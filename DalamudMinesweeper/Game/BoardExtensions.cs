using System;
using System.Linq;

namespace DalamudMinesweeper.Game;

public static class BoardExtensions
{
    public static Board From(Board board)
    {
        var result = new Board
        {
            width = board.width,
            height = board.height,
            cells = new Cell[board.width, board.height]
        };
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                var c = board.cells[x, y];
                result.cells[x, y] = new Cell
                {
                    numNeighbouringMines = c.numNeighbouringMines,
                    isFlagged = c.isFlagged,
                    isRevealed = c.isRevealed,
                    contents = c.contents
                };
            }
        }
        return result;
    }

    public static Board RevealAll(this Board board)
    {
        for (int x = 0; x < board.width; x++) {
            for (int y = 0; y < board.height; y++) {
                board.cells[x, y].isRevealed = true;
            }
        }
        return board;
    }

    public static Board HideAll(this Board board)
    {
        for (int x = 0; x < board.width; x++) {
            for (int y = 0; y < board.height; y++) {
                board.cells[x, y].isRevealed = false;
            }
        }
        return board;
    }

    public static CellContents RevealCell(this Board board, int x, int y)
    {
        board.cells[x, y].isRevealed = true;
        return board.GetCellContents(x, y);
    }

    public static CellContents GetCellContents(this Board board, int x, int y)
    {
        return board.cells[x, y].contents;
    }

    public static void SetCellContents(this Board board, int x, int y, CellContents contents)
    {
        board.cells[x, y].contents = contents;
    }

    public static bool ToggleCellFlag(this Board board, int x, int y)
    {
        if (board.cells[x, y].isRevealed) {
            return board.cells[x, y].isFlagged = false;
        }
        return board.cells[x, y].isFlagged = !board.cells[x, y].isFlagged;
    }

    public static bool CellIsFulfilled(this Board board, int x, int y)
    {
        var currentCell = board.cells[x, y];
        if (currentCell.contents != CellContents.Number || currentCell.isFlagged)
            return false;

        var neighbouringFlags = 0;
        
        // Loop through a square around the current cell
        for (int y2 = y-1; y2 <= y+1; y2++) {
            for (int x2 = x-1; x2 <= x+1; x2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= board.width || y2 >= board.height)
                    continue;
                
                var neighbourCell = board.cells[x2, y2];
                if (neighbourCell.isFlagged && !neighbourCell.isRevealed) {
                    neighbouringFlags++;
                }
            }
        }

        return neighbouringFlags == currentCell.numNeighbouringMines;
    }

    public static void ClickAdjacentUnflaggedTiles(this Board board, int x, int y, Action<int, int> clickAction)
    {
        var currentCell = board.cells[x, y];
        if (currentCell.contents != CellContents.Number || currentCell.isFlagged)
            return;
        
        // Loop through a square around the current cell
        for (int y2 = y-1; y2 <= y+1; y2++) {
            for (int x2 = x-1; x2 <= x+1; x2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= board.width || y2 >= board.height)
                    continue;
                
                var neighbourCell = board.cells[x2, y2];
                if (!neighbourCell.isFlagged && !neighbourCell.isRevealed)
                {
                    clickAction(x2, y2);
                }
            }
        }
        return;
    }

    public static void RevealConnectedField(this Board board, int x, int y)
    {
        var currentCell = board.cells[x, y];
        if (currentCell.contents != CellContents.Clear || currentCell.isFlagged)
            return;

        // Loop through a square around the current cell
        for (int y2 = y-1; y2 <= y+1; y2++) {
            for (int x2 = x-1; x2 <= x+1; x2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= board.width || y2 >= board.height)
                    continue;
                
                var neighbourCell = board.cells[x2, y2];
                if (neighbourCell.contents == CellContents.Clear && !neighbourCell.isRevealed) {
                    board.RevealCell(x2, y2);
                    RevealConnectedField(board, x2, y2);
                }
                if (neighbourCell.contents == CellContents.Number)
                    board.RevealCell(x2, y2);
            }
        }
        return;
    }

    public static bool IsVictory(this Board board)
    {
        var cells = board.cells.ToList();

        var minesAreFlagged = cells
            .Where(c => c.contents is CellContents.Mine)
            .All(c => c.isFlagged);

        var nonMinesRevealed = cells
            .Where(c => c.contents is not CellContents.Mine)
            .All(c => c.isRevealed);

        return minesAreFlagged && nonMinesRevealed;
    }
}