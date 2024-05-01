using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace DalamudMinesweeper.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration _configuration;

    public ConfigWindow(Plugin plugin) : base(
        "Minesweeper Settings",
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse |
        ImGuiWindowFlags.NoResize)
    {
        Size = new Vector2(390, 420);
        SizeCondition = ImGuiCond.Always;

        _configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Difficulty:");
        // Difficulty buttons
        if (ImGui.Button("Easy"))
        {
            _configuration.BoardWidth = 9;
            _configuration.BoardHeight = 9;
            _configuration.NumMines = 10;
        }
        ImGui.SameLine();
        if (ImGui.Button("Medium"))
        {
            _configuration.BoardWidth = 16;
            _configuration.BoardHeight = 16;
            _configuration.NumMines = 40;
        }
        ImGui.SameLine();
        if (ImGui.Button("Hard"))
        {
            _configuration.BoardWidth = 24;
            _configuration.BoardHeight = 24;
            _configuration.NumMines = 99;
        }
        ImGui.SameLine();
        if (ImGui.Button("Expert"))
        {
            _configuration.BoardWidth = 30;
            _configuration.BoardHeight = 16;
            _configuration.NumMines = 99;
        }

        // Config setters
        ImGui.PushItemWidth(150f * ImGuiHelpers.GlobalScale);
        var width = _configuration.BoardWidth;
        if (ImGui.InputInt("Board width", ref width))
        {
            if (width < 5) width = 5;
            if (width > 99) width = 99;
            _configuration.BoardWidth = width;
            
            _configuration.NumMines = Math.Min(MaxMines, _configuration.NumMines);
        }

        var height = _configuration.BoardHeight;
        if (ImGui.InputInt("Board height", ref height))
        {
            if (height < 5) height = 5;
            if (height > 99) height = 99;
            _configuration.BoardHeight = height;

            _configuration.NumMines = Math.Min(MaxMines, _configuration.NumMines);
        }

        var numMines = _configuration.NumMines;
        if (ImGui.InputInt("Number of mines", ref numMines))
        {
            if (numMines <= 1) numMines = 1;
            numMines = Math.Min(MaxMines, numMines);
            _configuration.NumMines = numMines;   
        }

        ImGui.Dummy(new Vector2(0, 30));

        //var devMode = _configuration.DevMode;
        //if (ImGui.Checkbox("Dev mode", ref devMode))
        //{
        //    _configuration.DevMode = devMode;
        //}

        ImGui.Text("No guess mode:");
        var noGuess = _configuration.NoGuess;
        if (ImGui.Checkbox("Enable no guess mode", ref noGuess))
        {
            _configuration.NoGuess = noGuess;
        }

        if (_configuration.NoGuess)
        {
            var noGuessTimeoutMs = _configuration.NoGuessTimeoutMs;
            if (ImGui.InputInt("Timeout (ms)", ref noGuessTimeoutMs, 100))
            {
                _configuration.NoGuessTimeoutMs = noGuessTimeoutMs;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("How long in milliseconds the algorithm is allowed when trying to generate a no guess board.\n" +
                    "Larger boards with more mines take exponentially more time.");
            }
        }
        else
        {
            ImGui.Dummy(new Vector2(0, 26));
        }

        ImGui.Dummy(new Vector2(0, 30));

        ImGui.Text("Shortcuts:");
        var revealShortcut = _configuration.RevealShortcut;
        if (ImGui.Checkbox("Left click on a number to automatically reveal adjacent tiles", ref revealShortcut))
        {
            _configuration.RevealShortcut = revealShortcut;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Only when the number already has the appropriate number of adjacent flags.");
        }
        var flagShortcut = _configuration.FlagShortcut;
        if (ImGui.Checkbox("Right click on a number to automatically place adjacent flags", ref flagShortcut))
        {
            _configuration.FlagShortcut = flagShortcut;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Only when the number has the appropriate number of adjacent hidden tiles.");
        }


        ImGui.Dummy(new Vector2(0, 37));


        if (ImGui.Button("Reset zoom level"))
        {
            _configuration.Zoom = 2;
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear scores"))
        {
            _configuration.Scores = new Game.Scores([]);
        }
        ImGui.SameLine();
        if (ImGui.Button("Save and Close")) {
            _configuration.Save();
            IsOpen = false;
        }
    }

    private int MaxMines => _configuration.BoardWidth * _configuration.BoardHeight - 9;
}
