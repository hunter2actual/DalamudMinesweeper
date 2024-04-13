using System.Collections.Generic;
using System.Linq;
using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;


public class TankSweeperStep
{
    private record LocatedCell(Cell Cell, int X, int Y);
    private record TankCell(int X, int Y, int NumRemainingFlags, TankCellType CellType);
    private enum TankCellType { BorderNumber, BorderHidden }

    /*
     * Considering groups of "border" tiles (hidden tiles adjacent to a number),
     * enumerate all possible configurations of mines in the group, and find
     * any flags which are common to all possibilities.
     * Should be slow but effective.
     */
    public static bool Step(MinesweeperGame game)
    {
        var tankCells = GetTankCells(game);
        var numPlacedFlags = GetNumFlags(game);
        var numRemainingMines = game.NumUnflaggedMines();
        if (tankCells.Count == 0)
            return false;

        var flagCombinations = FlagCombinations(tankCells.Count(tc => tc.CellType is TankCellType.BorderHidden))
            .Where(fc => fc.Count(x => x is true) <= numRemainingMines) // can't place more flags than we have remaining mines
            .ToList();

        
        // probably easier to say screw the data types, just calculate by iterating through if (x,y) 
        // is a borderNumber or BorderHidden and save those coords to two lists
        // then place hypothetical flags and evaluate game state


        return false;
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

    // Manipulation into a preferred data structure.
    // Only returns numbers bordering hidden cells, and hidden cells bordering numbers
    private static List<TankCell> GetTankCells(MinesweeperGame game)
    {
        var cells = new List<(LocatedCell lc, List<LocatedCell> neighbours, int x, int y)>();
        for (int x = 0; x < game.Board.width; x++) {
            for (int y = 0; y < game.Board.height; y++) {
                var cell = new LocatedCell(game.Board.cells[x, y], x, y);
                var neighbours = new List<LocatedCell>();
                for (int x2 = x-1; x2 <= x+1; x2++) {
                    for (int y2 = y-1; y2 <= y+1; y2++) {
                        // Skip self
                        if (x2 == x && y2 == y)
                            continue;
                        
                        // Avoid out of bounds
                        if (x2 < 0 || y2 < 0 || x2 >= game.Board.width || y2 >= game.Board.height)
                            continue;

                        neighbours.Add(new LocatedCell(game.GetCell(x2, y2), x2, y2));
                    }
                }
                cells.Add((cell, neighbours, x, y));
            }
        }

        var nonFlaggedHiddenBorderCells = cells
            .Where(x => x.lc.Cell.contents is CellContents.Number)
            .SelectMany(x => x.neighbours)
            .Where(n => n is {Cell.isRevealed: false, Cell.isFlagged: false})
            .ToHashSet();

        var borderNumberCells = cells
            .Where(x => x.lc.Cell is {isRevealed: true, contents: CellContents.Number})
            .Where(x => x.neighbours.Any(n => n is {Cell.isRevealed: false, Cell.isFlagged: false}))
            .Select(x => (locatedCell: x.lc, numNeighbouringFlags: x.neighbours.Count(n => n is {Cell.isFlagged: true})))
            .ToHashSet();

        var tankCells = new List<TankCell>();

        tankCells.AddRange(nonFlaggedHiddenBorderCells.Select(lc => new TankCell(
            lc.X,
            lc.Y,
            0,
            TankCellType.BorderHidden
        )));

        tankCells.AddRange(borderNumberCells.Select(b => new TankCell(
            b.locatedCell.X,
            b.locatedCell.Y,
            b.locatedCell.Cell.numNeighbouringMines - b.numNeighbouringFlags,
            TankCellType.BorderNumber
        )));

        return tankCells;
    }

    private static int GetNumFlags(MinesweeperGame game)
    {
        var numFlags = 0;
        for (int x = 0; x < game.Board.width; x++) {
            for (int y = 0; y < game.Board.height; y++) {
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

    private static bool IsBorderCell(int x, int y, MinesweeperGame game)
    {
        for (int x2 = x-1; x2 <= x+1; x2++) {
            for (int y2 = y-1; y2 <= y+1; y2++) {
                // Skip self
                if (x2 == x && y2 == y)
                    continue;
                
                // Avoid out of bounds
                if (x2 < 0 || y2 < 0 || x2 >= game.Board.width || y2 >= game.Board.height)
                    continue;

                var cell = game.GetCell(x2, y2);
                if (cell.contents is CellContents.Number && cell.isRevealed)
                    return true;
            }
        }
        return false;
    }
}
