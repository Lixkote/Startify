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

namespace Shell.Interface.StartMenu
{
    public sealed partial class StartMenuEntryApp : UserControl
    {
        public event EventHandler<RoutedEventArgs> TilePinned;
        public StartMenuEntryApp()
        {
            this.InitializeComponent();
        }

        private void RunAsAdminItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PinToTaskbar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PinToStartifyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string halal = CollapsedPathInformator.Text;
            TilePinned?.Invoke(halal, e);
        }

        private void UninstallAppMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
