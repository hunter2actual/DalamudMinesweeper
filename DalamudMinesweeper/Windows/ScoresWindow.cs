using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using DalamudMinesweeper.Game;
using Dalamud.Bindings.ImGui;

namespace DalamudMinesweeper.Windows;

public class ScoresWindow : Window, IDisposable
{
    private Configuration _configuration;

    public ScoresWindow(Plugin plugin) : base(
        "Minesweeper High Scores")
    {
        Size = new Vector2(350, 400);
        SizeCondition = ImGuiCond.Always;

        _configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (_configuration.Scores is null)
            _configuration.Scores = new Scores([]);

        var easy = new GameParameters(9, 9, 10);
        var medium = new GameParameters(16, 16, 40);
        var hard = new GameParameters(24, 24, 99);
        var expert = new GameParameters(16, 30, 99);

        var groups = _configuration.Scores.scores.OrderBy(x => x.time).GroupBy(x => x.parameters);

        var easyScores = groups.SingleOrDefault(g => g.First().parameters == easy);
        var mediumScores = groups.SingleOrDefault(g => g.First().parameters == medium);
        var hardScores = groups.SingleOrDefault(g => g.First().parameters == hard);
        var expertScores = groups.SingleOrDefault(g => g.First().parameters == expert);
        var otherScores = groups.Where(g => 
            g.First().parameters != easy
            && g.First().parameters != medium
            && g.First().parameters != hard
            && g.First().parameters != expert);

        if (expertScores is not null && expertScores.Any())
        {
            if (ImGui.CollapsingHeader("Expert " + GameParametersSummary(expert)))
            {
                foreach (var score in expertScores)
                {
                    ImGui.Text(TimeSummary(score.time));
                }
            }
        }
        if (hardScores is not null && hardScores.Any())
        {
            if (ImGui.CollapsingHeader("Hard " + GameParametersSummary(hard)))
            {
                foreach (var score in hardScores)
                {
                    ImGui.Text(TimeSummary(score.time));
                }
            }
        }
        if (mediumScores is not null && mediumScores.Any())
        {
            if (ImGui.CollapsingHeader("Medium " + GameParametersSummary(medium)))
            {
                foreach (var score in mediumScores)
                {
                    ImGui.Text(TimeSummary(score.time));
                }
            }
        }
        if (easyScores is not null && easyScores.Any())
        {
            if (ImGui.CollapsingHeader("Easy " + GameParametersSummary(easy)))
            {
                foreach (var score in easyScores)
                {
                    ImGui.Text(TimeSummary(score.time));
                }
            }
        }
        if (otherScores is not null && otherScores.Any())
        {
            if (ImGui.CollapsingHeader("Other"))
            {
                foreach (var group in otherScores)
                {
                    if (ImGui.TreeNode(GameParametersSummary(group.First().parameters)))
                    {
                        foreach (var score in group)
                        {
                            ImGui.Text(TimeSummary(score.time));
                        }
                        ImGui.TreePop();
                    }
                }
            }
        }
    }

    private string GameParametersSummary(GameParameters gp)
        => $"[{gp.width}x{gp.height} - {gp.numMines} mines]";

    private string TimeSummary(float time) => $"{time/1000:F3}s";
}
