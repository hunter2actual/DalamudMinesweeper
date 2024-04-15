# DalamudMinesweeper

Dalamud plugin for FFXIV, install by adding `https://raw.githubusercontent.com/hunter2actual/DalamudMinesweeper/master/DalamudMinesweeper.json` to your plugin sources.

How to play:
- Left click to uncover a square, right click to place a flag.
- Click a number that has the right amount of adjacent flags to reveal adjacent tiles.
- The game ends when all mines are flagged and all safe squares have been uncovered.
- Click the smiley face to start a new game.

Features:
- Score board
- No-guess mode!
- 3 difficulty presets

![Minesweeper plugin screenshot](/images/screenshot.png?raw=true "Minesweeper plugin screenshot")

### Sweeper things
- https://quantum-p.livejournal.com/19616.html
- https://web.mat.bham.ac.uk/R.W.Kaye/minesw/minesw.pdf
- https://web.mat.bham.ac.uk/R.W.Kaye/minesw/ASE2003.pdf
- https://massaioli.wordpress.com/2013/01/12/solving-minesweeper-with-matricies/
- https://luckytoilet.wordpress.com/2012/12/23/2125/

### Dev todos
- Active click logic (tile depression and shocked smiley)
- Refactoring, optimisation, more asyncs, clean up naming (cell/panel/tile etc)
- [Stretch] Skins