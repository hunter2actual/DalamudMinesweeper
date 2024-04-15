using DalamudMinesweeper.Sweepers;
using DalamudMinesweeper.Tests;
using System.Diagnostics;

namespace DalamudMinesweeper.Benchmarks;

using Difficulty = TestHelpers.Difficulty;

public class Program
{
    public static void Main(string[] args)
    {
        var task = SuccessRateTest();
    }

    public static async Task SuccessRateTest()
    {
        var sweeperBenchmarks = new SweeperBenchmarks();
        var limit = 5000;
        var stopwatch = new Stopwatch();

        foreach (var difficulty in new List<Difficulty> { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard })
        {
            var succeeded = 0;
            long successTime = 0;
            long failureTime = 0;

            for (int i = 0; i < limit; i++)
            {
                stopwatch.Restart();
                if (await SolveAsync(difficulty))
                {
                    stopwatch.Stop();
                    succeeded++;
                    successTime += stopwatch.ElapsedMilliseconds;
                }
                else
                {
                    stopwatch.Stop();
                    failureTime += stopwatch.ElapsedMilliseconds;
                }
            }

            var totalDuration = successTime + failureTime;
            successTime /= limit;
            failureTime /= limit;
            var percentage = Math.Round(100 * succeeded / (double)limit, 2);
            Console.WriteLine($"[{difficulty}] Solved in {succeeded}/{limit} times. {percentage}% success rate.");
            Console.WriteLine($"Total elapsed time: {totalDuration}ms");
            Console.WriteLine($"Average solve time: {successTime}ms");
            Console.WriteLine($"Average failure time: {failureTime}ms");
            Console.WriteLine();
        }
    }

    public static async Task<bool> SolveAsync(TestHelpers.Difficulty difficulty)
    {
        var game = TestHelpers.InitialiseGame(difficulty);
        var sweeper = new Sweeper();
        await sweeper.SweepAsync(game);
        return sweeper.Swept;
    }
}
