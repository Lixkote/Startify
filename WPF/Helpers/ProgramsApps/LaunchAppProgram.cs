using ShellApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.UI.Xaml.Controls;
using WPF.Views;

namespace WPF.Helpers.ProgramsApps
{
    internal class LaunchAppProgram
    {
        static async Task<AppListEntry> GetAppByPackageFamilyNameAsync(string packageFamilyName)
        {
            var pkgManager = new PackageManager();
            var pkg = pkgManager.FindPackagesForUser("", packageFamilyName).FirstOrDefault();

            if (pkg == null) return null;

            var apps = await pkg.GetAppListEntriesAsync();
            var firstApp = apps.FirstOrDefault();
            return firstApp;
        }
        public async void DirectoryAppLaunchHandler(object sender, ItemClickEventArgs e, Window window, ObservableCollection<StartMenuEntry> Programs)
        {
            StartMenuLink clickedItem = e.ClickedItem as StartMenuLink;
            string linkpath = clickedItem.Link;
            // Determine the target application type and launch it.
            window.Hide();
            if (linkpath != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(linkpath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Most likely win32 app launch was cancelled by the user." + "(Exception code: " + ex + ")");
                }
            }
            else
            {
                Debug.WriteLine("Launching UWP apps here is not supported/Directory child app launch failed.");
            }
        }
        public async void AppLaunchHandler(object sender, ItemClickEventArgs e, Window window, ObservableCollection<StartMenuEntry> Programs)
        {
            StartMenuEntry clickedItem = e.ClickedItem as StartMenuEntry;
            // Get the index of the clicked item in the ObservableCollection
            int index = Programs.IndexOf(clickedItem);

            // Get the path of the clicked item from the ObservableCollection
            string path = Programs[index].Path;
            string pathuwp = Programs[index].PathUWP;

            // Do something with the index and path
            window.Hide();
            if (path != null)
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            if (pathuwp != null)
            {
                var app = await GetAppByPackageFamilyNameAsync(pathuwp);
                if (app != null)
                {
                    await app.LaunchAsync();
                }
                else
                {
                    System.Windows.MessageBox.Show("This UWP app couldn't be launched.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        internal ItemClickEventHandler AppLaunchHandler(object sender, RoutedEventArgs e, StartMenu startMenu, ObservableCollection<StartMenuEntry> programs)
        {
            throw new NotImplementedException();
        }
    }
}
