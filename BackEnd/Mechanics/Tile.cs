using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace WPF.Helpers
{
    public class TileGroup : INotifyPropertyChanged
    {
        private string _name;
        private ObservableCollection<Tile> _tiles;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public ObservableCollection<Tile> Tiles
        {
            get { return _tiles; }
            set
            {
                if (_tiles != value)
                {
                    _tiles = value;
                    OnPropertyChanged(nameof(Tiles));
                }
            }
        }

        public TileGroup()
        {
            Tiles = new ObservableCollection<Tile>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Tile : INotifyPropertyChanged
    {
        private string _displayName;
        private string _appPath;
        private string _uwpId;
        private string _size;
        private BitmapImage _icon;
        private string _liveTileEnabled;
        private string _customTileBackground;

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string AppPath
        {
            get { return _appPath; }
            set
            {
                if (_appPath != value)
                {
                    _appPath = value;
                    OnPropertyChanged(nameof(AppPath));
                }
            }
        }

        public string UWPID
        {
            get { return _uwpId; }
            set
            {
                if (_uwpId != value)
                {
                    _uwpId = value;
                    OnPropertyChanged(nameof(UWPID));
                }
            }
        }

        public string Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public BitmapImage Icon
        {
            get { return _icon; }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        public string LiveTileEnabled
        {
            get { return _liveTileEnabled; }
            set
            {
                if (_liveTileEnabled != value)
                {
                    _liveTileEnabled = value;
                    OnPropertyChanged(nameof(LiveTileEnabled));
                }
            }
        }

        public string CustomTileBackground
        {
            get { return _customTileBackground; }
            set
            {
                if (_customTileBackground != value)
                {
                    _customTileBackground = value;
                    OnPropertyChanged(nameof(CustomTileBackground));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FolderTile : Tile
    {
        public ObservableCollection<Tile> Tiles { get; set; }

        public FolderTile()
        {
            Tiles = new ObservableCollection<Tile>();
        }
    }
}
