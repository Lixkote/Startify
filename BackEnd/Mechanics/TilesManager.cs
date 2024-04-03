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
                            try
                            {
                                Process.Start(new ProcessStartInfo(classicPath) { UseShellExecute = true });
                            }
                            catch(Exception ex)
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
                                DisplayName = Path.GetFileNameWithoutExtension(tileElement.Element("AppPath")?.Value),
                                AppPath = tileElement.Element("AppPath")?.Value,
                                Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(tileElement.Element("AppPath")?.Value))),
                                // Size = tileElement.Element("Size")?.Value,
                                Size = "Normal",
                                LiveTileEnabled = tileElement.Element("LiveTileEnabled")?.Value,
                                CustomTileBackground = tileElement.Element("CustomTileBackground")?.Value
                            }).ToList()
                        };

                        TileGroupscollection.Add(tileGroup);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Tiles Configuration XML was not found: " + ex.Message);
                }   
            }
            else
            {
                Debug.WriteLine("Tiles Configuration XML not found, attempting to create a new one.");
                CreateDefaultConfiguration(configFile);
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

        public void AddTileToXml(object path, ObservableCollection<TileGroup> tileGroups)
        {
            string configFile = GetConfigFilePath();
            XDocument doc = XDocument.Load(configFile);
            XElement rootElement = doc.Root;

            try
            {
                // Find an existing tile group or create a new one if not found
                XElement tileGroupElement = rootElement.Elements("TileGroup")
                                                       .FirstOrDefault(e => e.Attribute("Name")?.Value == "Default");

                if (tileGroupElement == null)
                {
                    // Create a new tile group element
                    tileGroupElement = new XElement("TileGroup",
                        new XAttribute("Name", "Default")
                    );
                    rootElement.Add(tileGroupElement);
                }

                // Add new tile to the existing or newly created tile group
                tileGroupElement.Add(
                    new XElement("Tile",
                        new XElement("AppPath", path as string),
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
