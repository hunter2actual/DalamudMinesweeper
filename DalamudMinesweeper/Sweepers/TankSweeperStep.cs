using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;

// TODOs:
// Segregate

public class TankSweeperStep
{
    private record LocatedCell(Cell Cell, int X, int Y);
    private record Point(int X, int Y);
    private record HypotheticalCell(int X, int Y, HypotheticalCellContents contents, int NumRemainingFlags);
    private enum HypotheticalCellContents { Irrelevant, HiddenUnsure, HiddenWithFlag, HiddenWithoutFlag, RevealedNumber }

    private static readonly int HiddenTileLimit = 14; // Exponential time, so this is important

    public static Task<bool> StepAsync(MinesweeperGame game, CancellationToken ct)
    {
        var preState = SweeperGameState.From(game);
        var numPlacedFlags = GetNumFlags(game);
        var numRemainingMines = game.NumUnflaggedMines();

        var (numbersBorderingHiddens, hiddensBorderingNumbers) = FindRelevantCells(game);
        if (numbersBorderingHiddens.Count == 0 || hiddensBorderingNumbers.Count == 0)
            return Task.FromResult(false);

        if (hiddensBorderingNumbers.Count >= HiddenTileLimit)
            return Task.FromResult(false);

        ct.ThrowIfCancellationRequested();

        var flagCombinations = FlagCombinations(hiddensBorderingNumbers.Count)
            .Where(fc => fc.Count(x => x is true) <= numRemainingMines || !fc.Any(x => x is true)) // can't place more flags than we have remaining mines
            .ToList();

        if (flagCombinations.Count == 0)
            return Task.FromResult(false);

        ct.ThrowIfCancellationRequested();

        var hypotheticals = CreateHypotheticals(game, numbersBorderingHiddens, hiddensBorderingNumbers, flagCombinations);

        if (hypotheticals.Count == 0)
            return Task.FromResult(false);

        var validHypotheticals = hypotheticals.Where(h => IsValidHypothetical(game, h)).ToList();

        if (validHypotheticals.Count == 0)
            return Task.FromResult(false);

        ct.ThrowIfCancellationRequested();

        var confirmedState = CondenseHypotheticals(game, validHypotheticals);

        ActOnState(game, confirmedState);

        var postState = SweeperGameState.From(game);

        return Task.FromResult(preState != postState);
    }

    /*
     * Considering groups of "border" tiles (hidden tiles adjacent to a number),
     * enumerate all possible configurations of mines in the group, and find
     * any flags which are common to all possibilities.
     * Should be slow but effective.
     */
    public static bool Step(MinesweeperGame game)
    {
        var preState = SweeperGameState.From(game);
        var numPlacedFlags = GetNumFlags(game);
        var numRemainingMines = game.NumUnflaggedMines();
    
        var (numbersBorderingHiddens, hiddensBorderingNumbers) = FindRelevantCells(game);
        if (numbersBorderingHiddens.Count == 0 || hiddensBorderingNumbers.Count == 0)
            return false;

        if (hiddensBorderingNumbers.Count >= HiddenTileLimit)
            return false;

        var flagCombinations = FlagCombinations(hiddensBorderingNumbers.Count)
            .Where(fc => fc.Count(x => x is true) <= numRemainingMines || !fc.Any(x => x is true)) // can't place more flags than we have remaining mines
            .ToList();

        if (flagCombinations.Count == 0)
            return false;

        var hypotheticals = CreateHypotheticals(game, numbersBorderingHiddens, hiddensBorderingNumbers, flagCombinations);

        if (hypotheticals.Count == 0)
            return false;

        var validHypotheticals = hypotheticals.Where(h => IsValidHypothetical(game, h)).ToList();

        if (validHypotheticals.Count == 0)
            return false;

        var confirmedState = CondenseHypotheticals(game, validHypotheticals);

        ActOnState(game, confirmedState);

        var postState = SweeperGameState.From(game);

        return preState != postState;
    }

    private static int GetNumFlags(MinesweeperGame game)
    {
        var numFlags = 0;
        for (int x = 0; x < game.Width; x++) {
            for (int y = 0; y < game.Height; y++) {
                if (game.GetCell(x, y).isFlagged) numFlags++;
            }
        }
        return numFlags;
    }

    private static (List<(Point point, int numRemainingFlags)> numbersBorderingHiddens, List<Point> hiddensBorderingNumbers) FindRelevantCells(MinesweeperGame game)
    {
        Cell currentCell;
        Cell neighbourCell;
        Point currentPoint;
        bool neighbourIsHidden;
        bool neighbourIsNumber;
        int numNeighbouringFlags;
        List<(Point point, int numRemainingFlags)> numbersBorderingHiddens = [];
        List<Point> hiddensBorderingNumbers = [];

        for (int x = 0; x < game.Width; x++)
        {
            for (int y = 0; y < game.Height; y++)
            {
                currentCell = game.GetCell(x, y);
                currentPoint = new Point(x, y);
                neighbourIsHidden = false;
                neighbourIsNumber = false;
                numNeighbouringFlags = 0;

                // Loop through a square around the current cell
                for (int x2 = x-1; x2 <= x+1; x2++) {
                    for (int y2 = y-1; y2 <= y+1; y2++) {
                        // Skip self
                        if (x2 == x && y2 == y)
                            continue;
                        
                        // Avoid out of bounds
                        if (x2 < 0 || y2 < 0 || x2 >= game.Width || y2 >= game.Height)
                            continue;
                        
                        neighbourCell = game.GetCell(x2, y2);

                        if (neighbourCell.isFlagged)
                        {
                            numNeighbouringFlags++;
                        }

                        if (!neighbourCell.isRevealed)
                        {
                            neighbourIsHidden = true;
                        }
                        else if (neighbourCell.contents is CellContents.Number)
                        {
                            neighbourIsNumber = true;
                        }
                    }
                }

                if (neighbourIsHidden && currentCell is { isRevealed: true, contents: CellContents.Number })
                {
                    var numRemainingFlags = currentCell.numNeighbouringMines - numNeighbouringFlags;
                    numbersBorderingHiddens.Add((currentPoint, numRemainingFlags));
                }
                else if (neighbourIsNumber && !currentCell.isRevealed && !currentCell.isFlagged)
                {
                    hiddensBorderingNumbers.Add(currentPoint);
                }
            }
        }

        return (numbersBorderingHiddens, hiddensBorderingNumbers);
    }

    // https://stackoverflow.com/questions/12488876/all-possible-combinations-of-boolean-variables
    private static List<bool[]> FlagCombinations(int numHiddenTiles)
    {
        var result = new List<bool[]>();

        if (numHiddenTiles < 1) return result;

        for (var i = 0; i < (1 << numHiddenTiles); ++i)
        {
            var combination = Enumerable.Range(0, numHiddenTiles).Select(j => (i & (1 << j)) != 0).ToArray();
            result.Add(combination);
        }
        return result;
    }

    // Returns a list of hypothetical boards in which only the relevant border cells are not null/default
    private static List<HypotheticalCell[,]> CreateHypotheticals(MinesweeperGame game, List<(Point point, int numRemainingFlags)> numbersBorderingHiddens, List<Point> hiddensBorderingNumbers, List<bool[]> flagCombinations)
    {
        if (flagCombinations.First().Length != hiddensBorderingNumbers.Count)
            throw new Exception("Tank step permutation mismatch");

        List<HypotheticalCell[,]> allHypotheticalBoards = [];

        HypotheticalCell[,] baseHypotheticalBoard = new HypotheticalCell[game.Width, game.Height];
        foreach (var nbh in numbersBorderingHiddens)
        {
            baseHypotheticalBoard[nbh.point.X, nbh.point.Y] = new HypotheticalCell(nbh.point.X, nbh.point.Y, HypotheticalCellContents.RevealedNumber, nbh.numRemainingFlags);
        }

        foreach (var fc in flagCombinations)
        {
            HypotheticalCell[,] currentHypotheticalBoard = (HypotheticalCell[,]) baseHypotheticalBoard.Clone();
            for (int i = 0; i < hiddensBorderingNumbers.Count; i++)
            {
                var hbn = hiddensBorderingNumbers[i];
                var contents = fc[i] ? HypotheticalCellContents.HiddenWithFlag : HypotheticalCellContents.HiddenWithoutFlag;
                currentHypotheticalBoard[hbn.X, hbn.Y] = new HypotheticalCell(hbn.X, hbn.Y, contents, 0);
            }
            allHypotheticalBoards.Add(currentHypotheticalBoard);
        }

        return allHypotheticalBoards;
    }

    private static bool IsValidHypothetical(MinesweeperGame game, HypotheticalCell[,] hypotheticalBoard)
    {
        for (int x = 0; x < game.Width; x++)
        {
            for (int y = 0; y < game.Height; y++)
            {
                if (hypotheticalBoard[x,y] is null || hypotheticalBoard[x, y].contents is not HypotheticalCellContents.RevealedNumber)
                    continue;

                var numRemainingFlags = hypotheticalBoard[x,y].NumRemainingFlags;

                // Loop through a square around the current cell
                for (int x2 = x-1; x2 <= x+1; x2++) {
                    for (int y2 = y-1; y2 <= y+1; y2++) {
                        // Skip self
                        if (x2 == x && y2 == y)
                            continue;
                        
                        // Avoid out of bounds
                        if (x2 < 0 || y2 < 0 || x2 >= game.Width || y2 >= game.Height)
                            continue;
                        
                        if (hypotheticalBoard[x2,y2] is null)
                            continue;

                        if (hypotheticalBoard[x2,y2].contents is HypotheticalCellContents.HiddenWithFlag)
                            numRemainingFlags--;
                    }
                }
                
                if (numRemainingFlags != 0) return false;
            }
        }
        return true;
    }

    private static HypotheticalCellContents[,] CondenseHypotheticals(MinesweeperGame game, List<HypotheticalCell[,]> boards)
    {
        HypotheticalCellContents[,] result = new HypotheticalCellContents[game.Width, game.Height];
        for (int x = 0; x < game.Width; x++) {
            for (int y = 0; y < game.Height; y++) {
                result[x,y] = HypotheticalCellContents.Irrelevant;
            }
        }

        foreach (var board in boards)
        {
            for (int x = 0; x < game.Width; x++) {
                for (int y = 0; y < game.Height; y++) {

                    switch (result[x,y], board[x,y]?.contents)
                    {
                        case (HypotheticalCellContents.HiddenUnsure, _):
                            continue;  // Unsure -> skip
                        case (HypotheticalCellContents.HiddenWithoutFlag, HypotheticalCellContents.HiddenWithFlag):
                        case (HypotheticalCellContents.HiddenWithFlag, HypotheticalCellContents.HiddenWithoutFlag):
                            result[x, y] = HypotheticalCellContents.HiddenUnsure; // Contradiction -> Unsure
                            break;
                        case (HypotheticalCellContents.Irrelevant, HypotheticalCellContents.HiddenWithoutFlag):
                        case (HypotheticalCellContents.HiddenWithoutFlag, HypotheticalCellContents.HiddenWithoutFlag):
                            result[x, y] = HypotheticalCellContents.HiddenWithoutFlag; // Agreement -> Keep
                            break;
                        case (HypotheticalCellContents.Irrelevant, HypotheticalCellContents.HiddenWithFlag):
                        case (HypotheticalCellContents.HiddenWithFlag, HypotheticalCellContents.HiddenWithFlag):
                            result[x, y] = HypotheticalCellContents.HiddenWithFlag; // Agreement -> Keep
                            break;
                        default:
                            continue;
                    }

                }
            } 
        }
        return result;
    }

    private static void ActOnState(MinesweeperGame game, HypotheticalCellContents[,] confirmedState)
    {
        for (int x = 0; x < game.Width; x++) {
            for (int y = 0; y < game.Height; y++) {

                switch (confirmedState[x, y])
                {
                    case HypotheticalCellContents.HiddenWithFlag:
                        game.RightClick(x, y);
                        break;
                    case HypotheticalCellContents.HiddenWithoutFlag:
                        game.Click(x, y);
                        break;
                    default:
                        break;
                }
            }
        } 
    }
}
