using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace DalamudMinesweeper.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration _configuration;

    public ConfigWindow(Plugin plugin) : base(
        "Minesweeper Settings",
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(350, 240);

        _configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Difficulty buttons
        if (ImGui.Button("Easy")) {
            _configuration.BoardWidth = 9;
            _configuration.BoardHeight = 9;
            _configuration.NumMines = 10;
        }
        ImGui.SameLine();
        if (ImGui.Button("Medium")) {
            _configuration.BoardWidth = 16;
            _configuration.BoardHeight = 16;
            _configuration.NumMines = 40;
        }
        ImGui.SameLine();
        if (ImGui.Button("Hard")) {
            _configuration.BoardWidth = 24;
            _configuration.BoardHeight = 24;
            _configuration.NumMines = 99;
        }

        // Config setters
        var width = _configuration.BoardWidth;
        if (ImGui.InputInt("Board width", ref width))
        {
            if (width < 5) width = 5;
            if (width > 99) width = 99;
            _configuration.BoardWidth = width;            
        }

        var height = _configuration.BoardHeight;
        if (ImGui.InputInt("Board height", ref height))
        {
            if (height < 5) height = 5;
            if (height > 99) height = 99;
            _configuration.BoardHeight = height;            
        }

        var numMines = _configuration.NumMines;
        if (ImGui.InputInt("Number of mines", ref numMines))
        {
            if (numMines <= 1) numMines = 1;
            numMines = Math.Min(_configuration.BoardWidth*_configuration.BoardHeight - 9, numMines);
            _configuration.NumMines = numMines;   
        }

        if (ImGui.Button("Reset zoom level"))
        {
            _configuration.Zoom = 2;
        }

        if (ImGui.Button("Clear scores"))
        {
            _configuration.Scores = new Game.Scores([]);
        }

        var devMode = _configuration.DevMode;
        if (ImGui.Checkbox("Dev mode", ref devMode))
        {
            _configuration.DevMode = devMode;
        }

        if (ImGui.Button("Save and Close")) {
            _configuration.Save();
            IsOpen = false;
        }
    }
}
