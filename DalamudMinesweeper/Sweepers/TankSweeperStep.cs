using System;
using System.Collections.Generic;
using System.Linq;
using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;


public class TankSweeperStep
{
    private record LocatedCell(Cell Cell, int X, int Y);
    private record TankCell(int X, int Y, int NumRemainingFlags, TankCellType CellType);
    private enum TankCellType { BorderNumber, BorderHidden }
    private record Point(int X, int Y);
    private record HypotheticalCell(int X, int Y, bool IsFlagged, int NumRemainingFlags);

    /*
     * Considering groups of "border" tiles (hidden tiles adjacent to a number),
     * enumerate all possible configurations of mines in the group, and find
     * any flags which are common to all possibilities.
     * Should be slow but effective.
     */
    public static bool Step(MinesweeperGame game)
    {
        // var tankCells = GetTankCells(game);
        var numPlacedFlags = GetNumFlags(game);
        var numRemainingMines = game.NumUnflaggedMines();
        // if (tankCells.Count == 0)
        //     return false;

        // var flagCombinations = FlagCombinations(tankCells.Count(tc => tc.CellType is TankCellType.BorderHidden))
        //     .Where(fc => fc.Count(x => x is true) <= numRemainingMines) // can't place more flags than we have remaining mines
        //     .ToList();

        
        // probably easier to say screw the data types, just calculate by iterating through if (x,y) 
        // is a borderNumber or BorderHidden and save those coords to two lists
        // then place hypothetical flags and evaluate game state

        var (numbersBorderingHiddens, hiddensBorderingNumbers) = FindRelevantCells(game);
        var flagCombinations = FlagCombinations(hiddensBorderingNumbers.Count);

        var hypotheticals = CreateHypotheticals(game, numbersBorderingHiddens, hiddensBorderingNumbers, flagCombinations);


        return false;
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
            baseHypotheticalBoard[nbh.point.X, nbh.point.Y] = new HypotheticalCell(nbh.point.X, nbh.point.Y, false, nbh.numRemainingFlags);
        }

        foreach (var fc in flagCombinations)
        {
            HypotheticalCell[,] currentHypotheticalBoard = (HypotheticalCell[,]) baseHypotheticalBoard.Clone();
            for (int i = 0; i < hiddensBorderingNumbers.Count; i++)
            {
                var hbn = hiddensBorderingNumbers[i];
                currentHypotheticalBoard[hbn.X, hbn.Y] = new HypotheticalCell(hbn.X, hbn.Y, fc[i], 0);
            }
            allHypotheticalBoards.Add(currentHypotheticalBoard);
        }

        return allHypotheticalBoards;
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
                else if (neighbourIsNumber && !currentCell.isRevealed)
                {
                    hiddensBorderingNumbers.Add(currentPoint);
                }
            }
        }

        return (numbersBorderingHiddens, hiddensBorderingNumbers);
    }

    /*
     * I need to:
     * Assign each visible number a numRemainingFlags DONE
     * calculate a set of all the border tiles (and a set of the numbers bordering them at the same time?) DONE
     * exit early if no border tiles DONE
     * (optional) find a way to segregate border tiles into different sets
     * Create all possible permutations of flags within the border tiles up to the number of remaining mines in the game DONE
     * cull flag permutations which are incompatible with any visible number's numRemainingFlags DONE
     * evaluate if a permutation satisfies the rules
     * check if anything in common between the permutations
     */

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
}
