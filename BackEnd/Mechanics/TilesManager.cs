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

        public void AddTileToXml(object path, ObservableCollection<TileGroup> tileGroupa)
        {
            string configFile = GetConfigFilePath();
            XDocument doc = XDocument.Load(configFile);

            XElement newTileElement = new XElement("Tile",
                new XElement("PathClassic", path as string),
                new XElement("PathImmersive", ""),
                new XElement("Size", "Normal"),
                new XElement("LiveTileEnabled", "false"),
                new XElement("CustomTileBackground", "")
            );

            var tileGroup = doc.Descendants("TileGroup").FirstOrDefault();
            if (tileGroup == null)
            {
                Debug.WriteLine($"TileGroup not found in XML.");
                return;
            }

            try
            {
                // Ensure that there is a "Tiles" element under the TileGroup
                var tilesElement = tileGroup.Element("Tiles");
                if (tilesElement == null)
                {
                    tilesElement = new XElement("Tiles");
                    tileGroup.Add(tilesElement);
                }

                // Add the new tile to the Tiles collection
                tilesElement.Add(newTileElement);
                doc.Save(configFile);

                // Update the TileList with the added tile
                foreach (XElement tileElementa in tilesElement.Descendants("Tile"))
                {
                    Tile tile = new Tile
                    {
                        DisplayName = Path.GetFileNameWithoutExtension(tileElementa.Element("PathClassic")?.Value),
                        PathClassic = tileElementa.Element("PathClassic")?.Value,
                        PathImmersive = tileElementa.Element("PathImmersive")?.Value,
                        Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(tileElementa.Element("PathClassic")?.Value))),
                        Size = tileElementa.Element("Size")?.Value,
                        LiveTileEnabled = tileElementa.Element("LiveTileEnabled")?.Value,
                        CustomTileBackground = tileElementa.Element("CustomTileBackground")?.Value
                    };

                    // Assuming tileGroupa is ObservableCollection<TileGroup>
                    var existingGroup = tileGroupa.FirstOrDefault();
                    if (existingGroup != null)
                    {
                        existingGroup.Tiles.Add(tile);
                    }
                    else
                    {
                        // Create a new TileGroup if it doesn't exist
                        TileGroup newGroup = new TileGroup
                        {
                            Name = tileGroup.Attribute("Name")?.Value,
                            Tiles = { tile }
                        };
                        tileGroupa.Add(newGroup);
                    }
                }

                Debug.WriteLine("Tile added to XML and TileList successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding tile to XML: {ex.Message}");
            }
        }




        public void RemoveTileFromXml(object sender)
        {
            // Your input string
            string input = sender.ToString();

            // Define the pattern for extracting paths
            string pattern = @"TilePathClassic:(.*?)TileGroupName:(.*?)$";

            // Use Regex to match the pattern
            Match match = Regex.Match(input, pattern);

            // Check if the match was successful
            if (match.Success)
            {
                string targetGroupName = match.Groups[2].Value.Trim();
                string TargetTilePathClassic = match.Groups[1].Value.Trim();

                string configFile = GetConfigFilePath();
                XDocument doc = XDocument.Load(configFile);

                // Adjust the targetGroupName based on your application logic to get the actual group name

                var tileGroup = doc.Descendants("TileGroup").FirstOrDefault(group => group.Attribute("Name")?.Value == targetGroupName);

                if (tileGroup == null)
                {
                    Debug.WriteLine($"TileGroup '{targetGroupName}' not found in XML.");
                    return;
                }

                var tileElement = tileGroup.Elements("Tiles")
                    .Elements("Tile")
                    .FirstOrDefault(t => String.Equals(t.Element("PathClassic")?.Value, TargetTilePathClassic, StringComparison.OrdinalIgnoreCase));

                if (tileElement != null)
                {
                    tileElement.Remove();
                    doc.Save(configFile);
                    Debug.WriteLine("Tile removed from XML successfully.");
                }
                else
                {
                    Debug.WriteLine($"Tile '{TargetTilePathClassic}' not found in XML.");
                }
            }
        }

        public string GetConfigFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Startify", "Tiles", "Layout.xml");
        }

    }
}
