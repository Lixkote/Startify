using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;

namespace WPF.Helpers
{
    public class TileGroup
    {
        public string Name { get; set; }
        public List<Tile> Tiles { get; set; }

        public TileGroup()
        {
            Tiles = new List<Tile>();
        }
    }

    public class Tile
    {
        public string DisplayName { get; set; }
        public string PathClassic { get; set; }
        public string PathImmersive { get; set; }
        public string Size { get; set; }
        public Windows.UI.Xaml.Media.Imaging.BitmapImage Icon { get; set; }
        public string LiveTileEnabled { get; set; }
        public string CustomTileBackground { get; set; }
    }

    public class FolderTile : Tile
    {
        public List<Tile> Tiles { get; set; }
    }
}
