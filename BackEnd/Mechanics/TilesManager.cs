using Shell.Interface.StartMenu;
using ShellApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.UI.Xaml.Controls;


namespace WPF.Helpers
{
    internal class TilesManager
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
        public async void OpenTileApp(object sender, Windows.UI.Xaml.RoutedEventArgs e, Window window, bool runasadmin)
        {
            // Your input string
            string input = sender as string;

            // Define the pattern for extracting paths
            string pattern = @"Classic Path:(.*?)Immersive Path:(.*?)$";

            // Use Regex to match the pattern
            Match match = Regex.Match(input, pattern);

            // Check if the match was successful
            if (match.Success)
            {
                // Extract the paths from the matched groups
                string classicPath = match.Groups[1].Value.Trim();
                string immersivePath = match.Groups[2].Value.Trim();
                if (classicPath != null)
                {
                    if (classicPath != "")
                    {
                        if (runasadmin == false)
                        {
                            Process.Start(new ProcessStartInfo(classicPath) { UseShellExecute = true });
                        }
                        else if (runasadmin == true)
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo(classicPath)
                            {
                                UseShellExecute = true,
                                Verb = "runas" // This will request admin privileges
                            };
                            Process.Start(startInfo);
                        }
                    }
                }
                if (immersivePath != null)
                {
                    if (immersivePath != "")
                    {
                        var app = await GetAppByPackageFamilyNameAsync(immersivePath);
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
                window.Hide();
            }
            else
            {
                Console.WriteLine("Pattern not found in the input string.");
                Console.WriteLine("Fatal error opening app.");
                window.Hide();
            }
        }


        public event EventHandler<EventArgs> CouldNotLoadTiles;
        public void LoadTileGroups(ObservableCollection<TileGroup> TileGroupscollection)
        {
            string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Startify", "Tiles", "Layout.xml");

            if (File.Exists(configFile))
            {
                try
                {
                    XDocument doc = XDocument.Load(configFile);

                    foreach (XElement tileGroupElement in doc.Descendants("TileGroup"))
                    {
                        TileGroup tileGroup = new TileGroup
                        {
                            Name = tileGroupElement.Attribute("Name")?.Value,
                            Tiles = tileGroupElement.Descendants("Tile").Select(tileElement => new Tile
                            {
                                DisplayName = Path.GetFileNameWithoutExtension(tileElement.Element("PathClassic")?.Value),
                                PathClassic = tileElement.Element("PathClassic")?.Value,
                                PathImmersive = tileElement.Element("PathImmersive")?.Value,
                                Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(tileElement.Element("PathClassic")?.Value))),
                                Size = tileElement.Element("Size")?.Value,
                                LiveTileEnabled = tileElement.Element("LiveTileEnabled")?.Value,
                                CustomTileBackground = tileElement.Element("CustomTileBackground")?.Value
                            }).ToList()
                        };

                        TileGroupscollection.Add(tileGroup);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error loading tiles Configuration XML: " + ex.Message);
                    CouldNotLoadTiles(this, null);
                }
            }
            else
            {
                Debug.WriteLine("Tiles configuration file not found.");
                CouldNotLoadTiles(this, null);
            }
        }

    }
}
