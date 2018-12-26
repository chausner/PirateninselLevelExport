# PirateninselLevelExport
Tool for viewing and editing levels in the 1997 game "Die Pirateninsel" by Sunflowers.

Features
--------
* Export levels from the game's internal format to [Tiled](https://www.mapeditor.org/) level format
* Import levels created or edited in Tiled back into the game
* Generate images of all levels in the game

Usage
-----
**Prerequisites:** you need the original game files. The game runs well in [DOSBox](https://www.dosbox.com/) on modern machines.

### Exporting levels
```
PirateninselLevelExport.exe export --game-dir=C:\PIRAT --tiled-dir=.\levels
```
Levels will be converted to Tiled level format and stored in a subdirectory ```levels```. You can now view and edit the levels using Tiled.

### Importing levels
```
PirateninselLevelExport.exe import --game-dir=C:\PIRAT --tiled-dir=.\levels
```
Levels will be converted back to the game's internal format and stored within the game directory. The original level files are overwritten, so it may be a good idea to back them up before running.

### Generate images of levels
```
PirateninselLevelExport.exe gen-level-images --game-dir=C:\PIRAT --dest-dir=.\level-imgs
```
All levels will be saved as PNG images in the ```level-imgs``` subdirectory.

License
-------
MIT, see LICENSE
