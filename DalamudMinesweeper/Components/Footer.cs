using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using DalamudMinesweeper.Game;
using ImGuiNET;

namespace DalamudMinesweeper.Components;

public class Footer
{
    private Configuration _configuration;
    private Action _drawConfigAction;

    public Footer(Configuration configuration, Action drawConfigAction)
    {
        _configuration = configuration;
        _drawConfigAction = drawConfigAction;
    }

    public void Draw(Vector2 start)
    {
        ImGui.SetCursorPos(start);

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
        {
            _drawConfigAction();
        }        
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus) && _configuration.Zoom <= 5)
        {
            _configuration.Zoom++;
        }
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Minus) && _configuration.Zoom > 1)
        {
            _configuration.Zoom--;
        };
        ImGui.SameLine();
    }
}
