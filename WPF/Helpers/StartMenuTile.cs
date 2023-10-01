using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;

namespace WPF.Helpers
{
    internal abstract class StartMenuTile
    {
        public string TileName { get; set; }
        public string Path { get; set; }
        public string Link { get; set; }    
        public string PathUWP { get; set; }
        public Windows.UI.Xaml.Media.Imaging.BitmapImage TileIcon { get; set; }
    }
    internal class StartMenuTileDirectory : StartMenuTileLink
    {
        public bool HasChildren { get; set; }
        public ObservableCollection<StartMenuTileLink> Links { get; set; }
        public ObservableCollection<StartMenuTileDirectory> Directories { get; set; }
    }
    internal class StartMenuTileLink : StartMenuTile
    {
        public string Link { get; set; }
        public string LinkUWP { get; set; }
        public bool AllowOpenLocation { get; set; }
    }
}
