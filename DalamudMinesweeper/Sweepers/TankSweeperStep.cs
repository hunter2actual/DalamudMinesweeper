using DalamudMinesweeper.Game;

namespace DalamudMinesweeper.Sweepers;

public class TankSweeperStep
{
    /*
     * Considering groups of "border" tiles (hidden tiles adjacent to a number),
     * enumerate all possible configurations of mines in the group, and find
     * any flags which are common to all possibilities
     */
    public static bool Step(MinesweeperGame game)
    {
        return false;
    }
}
