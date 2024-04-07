using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class Footer
{
    private Configuration _configuration;
    private Action _drawConfigAction;
    private Action _drawScoresAction;
    private Action _solverStepAction;

    public Footer(Configuration configuration, Action drawConfigAction, Action drawScoresAction, Action solverStepAction)
    {
        _configuration = configuration;
        _drawConfigAction = drawConfigAction;
        _drawScoresAction = drawScoresAction;
        _solverStepAction = solverStepAction;
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
            if (ImGui.Button("Solver Step"))
            {
                _solverStepAction();
            }
        }
    }
}
