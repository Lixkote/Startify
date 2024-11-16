using Shell.Interface.StartMenu;
using ShellApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using WPF.Views;


namespace WPF.Helpers
{
    internal class TilesManager
    {
        private Windows.Foundation.Size _logoSize;
        RandomAccessStreamReference logoData;
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
            var tilegroupcontrol = sender as Button;
            var tile = tilegroupcontrol.DataContext as Tile;
            // Extract the paths from the matched groups
            string classicPath = tile.AppPath;
            string immersivePath = tile.UWPID;
            if (classicPath != null)
            {
                if (classicPath != "")
                {
                    if (runasadmin == false)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(classicPath) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            ModernWpf.MessageBox.Show(ex.ToString(), "Sorry, but we couldn't launch this app.", MessageBoxButton.OK, ModernWpf.SymbolGlyph.Asterisk);
                        }
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
            StartMenu11 startMenu11 = window as StartMenu11;
            startMenu11.HideStartMenuNoEvent();
        }

        public event EventHandler<EventArgs> CouldNotLoadTiles;
        public async Task LoadTileGroups(ObservableCollection<TileGroup> TileGroupscollection)
        {
            string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Startify", "Tiles", "Layout.xml");

            if (!File.Exists(configFile))
            {
                Debug.WriteLine("Tiles Configuration XML not found, attempting to create a new one.");
                CreateDefaultConfiguration(configFile);
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(configFile);

                PackageManager packageManager = new PackageManager();
                var packages = packageManager.FindPackagesForUser("");

                var tileGroupTasks = doc.Descendants("TileGroup").Select(async tileGroupElement =>
                {
                    TileGroup tileGroup = new TileGroup
                    {
                        Name = tileGroupElement.Attribute("Name")?.Value,
                        Tiles = new ObservableCollection<Tile>()
                    };

                    var tileTasks = tileGroupElement.Descendants("Tile").Select(async tileElement =>
                    {
                        Tile tile = new Tile
                        {
                            AppPath = tileElement.Element("AppPath")?.Value,
                            UWPID = tileElement.Element("UWPID")?.Value,
                            Size = "Normal",
                            LiveTileEnabled = tileElement.Element("LiveTileEnabled")?.Value,
                            CustomTileBackground = tileElement.Element("CustomTileBackground")?.Value
                        };

                        if (string.IsNullOrEmpty(tile.UWPID))
                        {
                            tile.Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(tile.AppPath)));
                            tile.DisplayName = Path.GetFileNameWithoutExtension(tile.AppPath);
                        }
                        else
                        {
                            var matchingPackage = packages.FirstOrDefault(package => package.Id.FamilyName.Equals(tile.UWPID, StringComparison.OrdinalIgnoreCase));
                            if (matchingPackage != null)
                            {
                                var appListEntries = await matchingPackage.GetAppListEntriesAsync();
                                var appListEntry = appListEntries.FirstOrDefault();
                                if (appListEntry != null)
                                {
                                    tile.DisplayName = appListEntry.DisplayInfo.DisplayName;

                                    try
                                    {
                                        var logoData = matchingPackage.GetLogoAsRandomAccessStreamReference(new Windows.Foundation.Size(32, 32));
                                        var stream = await logoData.OpenReadAsync();
                                        var bitmapImage = new BitmapImage();
                                        await bitmapImage.SetSourceAsync(stream);
                                        tile.Icon = bitmapImage;
                                    }
                                    catch
                                    {
                                        tile.Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                                    }
                                }
                            }
                        }
                        return tile;
                    });
                    foreach (var task in tileTasks)
                    {
                        tileGroup.Tiles.Add(await task);
                    }

                    return tileGroup;
                });

                var loadedTileGroups = await Task.WhenAll(tileGroupTasks);
                foreach (var tileGroup in loadedTileGroups)
                {
                    TileGroupscollection.Add(tileGroup);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Tiles Configuration XML was not found: " + ex.Message);
            }
        }





        private void CreateDefaultConfiguration(string configFile)
        {
            try
            {
                // Attempt to load the existing configuration file
                XDocument doc = XDocument.Load(configFile);
            }
            catch (Exception ex)
            {
                // If the file is not found, create a new one with default content
                string directoryPath = Path.GetDirectoryName(configFile);
                if (!Directory.Exists(directoryPath))
                {
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(directoryPath);
                }

                // Create default configuration
                XDocument doc = new XDocument(
                    new XElement("XmlRoot")
                );

                // Save the default configuration to the specified path
                doc.Save(configFile);
            }
        }

        public void AddTileToXml(object sender, ObservableCollection<TileGroup> tileGroups)
        {
            var linkToAdda = sender as MenuFlyoutItem;
            var tileGroup = linkToAdda.DataContext as StartMenuLink;
            string configFile = GetConfigFilePath();
            XDocument doc = XDocument.Load(configFile);
            XElement rootElement = doc.Root;

            try
            {
                // Find an existing tile group or create a new one if not found
                XElement tileGroupElement = rootElement.Elements("TileGroup")
                                                       .FirstOrDefault();

                if (tileGroupElement == null)
                {
                    // Create a new tile group element
                    tileGroupElement = new XElement("TileGroup",
                        new XAttribute("Name", "")
                    );
                    rootElement.Add(tileGroupElement);
                }

                // Add new tile to the existing or newly created tile group
                tileGroupElement.Add(
                    new XElement("Tile",
                        new XElement("AppPath", tileGroup.Path),
                        new XElement("UWPID", tileGroup.PathUWP),
                        new XElement("Size", "Normal"),
                        new XElement("LiveTileEnabled", "false"),
                        new XElement("CustomTileBackground", "")
                    )
                );

                // Save changes to the XML document
                doc.Save(configFile);
                Debug.WriteLine("Tile added to XML and TileList successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding tile to XML: {ex.Message}");
            }
        }

        public void ChangeGroupName(object sender, ObservableCollection<TileGroup> tileGroups)
        {
            var groupNameBox = sender as Shell.Interface.StartMenu11.Controls.TileGroupNameBox;
            string newGroupName = groupNameBox.Text;

            // Get the DataContext of the TextBox, which should be the TileGroup object
            var tileGroup = groupNameBox.DataContext as TileGroup;

            if (tileGroup == null)
            {
                Debug.WriteLine("DataContext is not TileGroup.");
                return;
            }

            // Store the original name before changing
            string originalName = tileGroup.Name;

            if (string.IsNullOrEmpty(newGroupName))
            {
                Debug.WriteLine("New group name is empty or null.");
                return;
            }

            // Update the name in the TileGroup object
            tileGroup.Name = newGroupName;
            Debug.WriteLine($"Group name updated from '{originalName}' to '{newGroupName}'.");

            // Update the name in the layout XML file
            UpdateGroupNameInXml(originalName, newGroupName);

            // Optionally, you can update the collection if necessary
            // (not needed if ObservableCollection is bound to UI and Two-Way binding is enabled)
            // var index = tileGroups.IndexOf(tileGroup);
            // tileGroups[index] = tileGroup;
        }

        private void UpdateGroupNameInXml(string originalName, string newGroupName)
        {
            string configFile = GetConfigFilePath();
            XDocument doc = XDocument.Load(configFile);

            var tileGroupElement = doc.Descendants("TileGroup")
                                      .FirstOrDefault(group => group.Attribute("Name")?.Value == originalName);

            if (tileGroupElement != null)
            {
                tileGroupElement.Attribute("Name").Value = newGroupName;
                doc.Save(configFile);
                Debug.WriteLine($"Group name updated in XML from '{originalName}' to '{newGroupName}'.");
            }
            else
            {
                Debug.WriteLine($"TileGroup '{originalName}' not found in the XML.");
            }
        }

        public void ChangeTileSizeInXML(object sender, RoutedEventArgs e)
        {
            string input = sender.ToString();

            string pattern = @"TilePathClassic:(.*?)TileGroupName:(.*?)$";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string targetGroupName = match.Groups[2].Value.Trim();
                string targetTilePathClassic = match.Groups[1].Value.Trim();

                string configFile = GetConfigFilePath();
                XDocument doc = XDocument.Load(configFile);

                XElement rootElement = doc.Root;

                try
                {
                    XElement tileGroupElement = rootElement.Elements("TileGroup")
                                                           .FirstOrDefault(e => e.Attribute("Name")?.Value == targetGroupName);

                    if (tileGroupElement == null)
                    {
                        Debug.WriteLine($"TileGroup '{targetGroupName}' not found in XML.");
                        return;
                    }

                    var tileElement = tileGroupElement.Elements("Tile")
                                                       .FirstOrDefault(t => String.Equals(t.Element("AppPath")?.Value, targetTilePathClassic, StringComparison.OrdinalIgnoreCase));

                    if (tileElement != null)
                    {
                        // tileElement.Attribute("Size").Value = newGroupName;
                        doc.Save(configFile);
                        Debug.WriteLine("Tile size changed from XML successfully.");
                    }
                    else
                    {
                        Debug.WriteLine($"Tile '{targetTilePathClassic}' not found in XML.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error changing size in tile from XML: {ex.Message}");
                }
            }
        }



        public void RemoveTileFromXml(object path, ObservableCollection<TileGroup> tileGroups)
        {
            string input = path.ToString();

            string pattern = @"TilePathClassic:(.*?)TileGroupName:(.*?)$";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string targetGroupName = match.Groups[2].Value.Trim();
                string targetTilePathClassic = match.Groups[1].Value.Trim();

                string configFile = GetConfigFilePath();
                XDocument doc = XDocument.Load(configFile);

                XElement rootElement = doc.Root;

                try
                {
                    XElement tileGroupElement = rootElement.Elements("TileGroup")
                                                           .FirstOrDefault(e => e.Attribute("Name")?.Value == targetGroupName);

                    if (tileGroupElement == null)
                    {
                        Debug.WriteLine($"TileGroup '{targetGroupName}' not found in XML.");
                        return;
                    }

                    var tileElement = tileGroupElement.Elements("Tile")
                                                       .FirstOrDefault(t => String.Equals(t.Element("AppPath")?.Value, targetTilePathClassic, StringComparison.OrdinalIgnoreCase));

                    if (tileElement != null)
                    {
                        tileElement.Remove();
                        doc.Save(configFile);
                        Debug.WriteLine("Tile removed from XML successfully.");
                    }
                    else
                    {
                        Debug.WriteLine($"Tile '{targetTilePathClassic}' not found in XML.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error removing tile from XML: {ex.Message}");
                }
            }
        }


        public string GetConfigFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Startify", "Tiles", "Layout.xml");
        }

    }
}
