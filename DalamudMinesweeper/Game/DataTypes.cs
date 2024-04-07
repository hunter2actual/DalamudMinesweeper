using System.Collections.Generic;

namespace DalamudMinesweeper.Game;

public record Board {
    public Cell[,] cells = new Cell[,]{};
    public int width;
    public int height;
}

public record Cell {
    public int numNeighbouringMines;
    public bool isRevealed;
    public bool isFlagged;
    public CellContents contents;
}

public enum CellContents { Clear, Number, Mine, ExplodedMine }

public enum GameState { Playing, Victorious, Boom }

public record Scores(List<(GameParameters parameters, long time)> scores);

public record GameParameters(int width, int height, int numMines);