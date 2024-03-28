using System.Collections.Generic;

namespace DalamudMinesweeper.Game;

public static class Array2dExtensions {
    public static List<T> ToList<T>(this T[,] array)
    {
        var result = new List<T>();
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                result.Add(array[i, j]);
            }
        }
        return result;
    }
}