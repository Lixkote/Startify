﻿using Shell.Shell.ShellUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Szablon elementu Kontrolka użytkownika jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234236

namespace Shell.Interface.StartMenu11.Controls
{
    public sealed partial class TileGroupControl : UserControl
    {
        public event EventHandler<RoutedEventArgs> TileClicked;
        public event EventHandler<RoutedEventArgs> TileUnpinned;
        public event EventHandler<RoutedEventArgs> TileGroupNameChanged;
        public event EventHandler<RoutedEventArgs> TileSizeChanged;
        public TileGroupControl()
        {
            this.InitializeComponent();

        }
        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            TileClicked?.Invoke(sender, e);
        }

        private void Tile_UnpinTile(object sender, RoutedEventArgs e)
        {
            string TileGroupTileName = "TilePathClassic:" + sender + "TileGroupName:" + TileGridViewGroupName.Text;
            TileUnpinned?.Invoke(TileGroupTileName, e);
        }

        private void TileGridViewGroupName_LostFocus(object sender, RoutedEventArgs e)
        {
            TileGroupNameChanged?.Invoke(sender, e);
        }

        private void Tile_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TileSizeChanged?.Invoke(sender, e);
        }
    }
}
