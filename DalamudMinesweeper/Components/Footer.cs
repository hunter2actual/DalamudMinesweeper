using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using DalamudMinesweeper.Game;
using DalamudMinesweeper.Sweepers;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class Footer
{
    public MinesweeperGame Game { get; set; }
    private Configuration _configuration;
    private Action _drawConfigAction;
    private Action _drawScoresAction;
    private readonly Sweeper _sweeper;
    private bool? _swept;

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
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
        {
            _drawConfigAction();
        }        
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus) && _configuration.Zoom < 5)
        {
            _configuration.Zoom++;
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Minus) && _configuration.Zoom > 1)
        {
            _configuration.Zoom--;
        }
        ImGui.SameLine();

        if (_configuration.DevMode)
        {
            if (ImGui.Button("Simple"))
            {
                _swept = _sweeper.SimpleSweep(Game);
            }
            ImGui.SameLine();
            if (ImGui.Button("Tank"))
            {
                _swept = _sweeper.TankSweep(Game);
            }
            ImGui.SameLine();
            if (_swept is not null)
            {
               ImGui.Text($"{(_swept.Value ? "Swept" : "Stalled" )} after {_sweeper.NumSteps} steps in {_sweeper.Stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }
}
