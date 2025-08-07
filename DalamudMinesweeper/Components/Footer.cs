using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sweepers;
using Dalamud.Bindings.ImGui;

namespace DalamudMinesweeper.Components;

public class Footer
{
    public MinesweeperGame Game { get; set; }
    private Configuration _configuration;
    private Action _drawConfigAction;
    private Action _drawScoresAction;
    private readonly Sweeper _sweeper;

    public Footer(MinesweeperGame game, Configuration configuration, Action drawConfigAction, Action drawScoresAction, Sweeper sweeper)
    {
        Game = game;
        _configuration = configuration;
        _drawConfigAction = drawConfigAction;
        _drawScoresAction = drawScoresAction;
        _sweeper = sweeper;
    }

    public void Draw(Vector2 start)
    {
        ImGui.SetCursorPos(start);

        if (ImGuiComponents.IconButton(FontAwesomeIcon.ListOl))
        {
            _drawScoresAction();
        };
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("View high scores");
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
        {
            _drawConfigAction();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Settings");
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus) && _configuration.Zoom < 5)
        {
            _configuration.Zoom++;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Zoom in");
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Minus) && _configuration.Zoom > 1)
        {
            _configuration.Zoom--;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Zoom out");
        }
        ImGui.SameLine();

        if (!Game.NoGuessValid)
        {
            ImGui.Text("[No-guess failed]");
        }


        //ImGui.SameLine();
        //if (_configuration.DevMode)
        //{
        //    if (ImGui.Button("Sweep"))
        //    {
        //        _sweeper.SweepAsync(Game);
        //    }
        //    ImGui.SameLine();
        //    ImGui.Text($"{(_sweeper.Swept ? "Swept" : "Stalled")} after {_sweeper.NumSimpleSteps} simple steps and {_sweeper.NumTankSteps} tank steps in {_sweeper.Stopwatch.ElapsedMilliseconds}ms");
        //}
    }
}
