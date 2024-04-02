using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Shell.Interface.StartMenu11.Controls
{
    public sealed partial class AllAppsPaneControl : UserControl
    {
        public event EventHandler<ItemClickEventArgs> DirectoryChildClickeda;
        public event EventHandler<RoutedEventArgs> UninstallSettingsShouldOpen;
        public event EventHandler<RoutedEventArgs> OpenFileLocationClicked;
        public event EventHandler<RoutedEventArgs> TileClickedMain;
        public event EventHandler<RoutedEventArgs> TilePinnedMain;
        public event EventHandler<RoutedEventArgs> TileUnpinnedMain;
        public AllAppsPaneControl()
        {
            this.InitializeComponent();
            alphabetlist.ItemsSource = new ObservableCollection<string>("&#ABCDEFGHIJKLMNOPQRSTUVWXYZ".Select(c => c.ToString()));
        }

        private void DirectoryChildContainer_ItemClick(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            DirectoryChildClickeda?.Invoke(sender, e);
        }
        private void StartMenuEntryApp_TilePinned(object sender, RoutedEventArgs e)
        {
            TilePinnedMain?.Invoke(sender, e);
        }
        private void UninstallAppMenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            UninstallSettingsShouldOpen?.Invoke(sender, e);
        }

        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            OpenFileLocationClicked?.Invoke(sender, e);
        }

        private void RunAsAdminItem_Click(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            // RunAsAdminClicked?.Invoke(sender, e);
        }
    }
}
