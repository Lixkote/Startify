using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace Shell.Shell.Start.Tiles
{
    public sealed partial class TileGroupControl : UserControl
    {
        public event EventHandler<ItemClickEventArgs> TileClicked;
        public TileGroupControl()
        {
            this.InitializeComponent();
        }

        public void TileGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            TileClicked?.Invoke(sender, e);
        }
    }
}
