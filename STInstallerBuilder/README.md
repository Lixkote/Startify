### Startify Configuration Template

When, manually compiling startify, these files need to be moved into C:\Users\yourusername\Startify\.

The startify control panel app, changes these settings from a gui, and then when launching startify these settings get loaded into it.

#### Description of settings in the current version as of 23.07.2024
  
-   Settings.cfg:
    -   DockedDesign (bool) - This toggles between floating and docked start menu style. 
    - DisplayTiles (bool) - Display or hide tiles part of the start menu
    - Show*Button (bool) - Displays or hides the respective button in left bar of the start menu.
    - TooltipCaption (string) - Tooltip caption to search for when hooking the Start Button (This should be Start, in your OS' language (example: "Démarrer" if using a French language pack))
    - TooltipName (string) - Tooltip Window name to search for when hooking the Start Button (Internally used as the window class, should be "Start", even on non-English systems)
 
      Be careful about the two last ones if you are using any other language than English or Polish

- Tiles\Layout.xml
	- This is a xml layout tree of pinned tiles on the start menu. 
	Properties: 
	  - Size (Normal, Small, Wide, Large) - the size of tile to display
	  - CustomTileBackground - Background png (or an animated gif) image path or hex value of color to use as tile background
	  - LiveTileEnabled - placeholder for now
	  - UWPID & AppPath - Specifies the app path or uwp identification if of pinned application.
