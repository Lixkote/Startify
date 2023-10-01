using ShellApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.UI.Xaml.Controls;


namespace WPF.Helpers.ProgramsApps
{
    internal class TileGetHelper
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
        public async void TileLaunchHandler(object sender, ItemClickEventArgs e, Window window, ObservableCollection<StartMenuTile> Programs, bool runasadmin)
        {
            StartMenuTile clickedItem = e.ClickedItem as StartMenuTile;
            // Get the index of the clicked item in the ObservableCollection
            int index = Programs.IndexOf(clickedItem);

            // Get the path of the clicked item from the ObservableCollection
            string path = Programs[index].Path;
            string pathuwp = Programs[index].PathUWP;

            // Do something with the index and path
            window.Hide();
            if (path != null)
            {
                if (runasadmin == false)
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else if (runasadmin == true)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(path)
                    {
                        UseShellExecute = true,
                        Verb = "runas" // This will request admin privileges
                    };
                    Process.Start(startInfo);
                }
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
        public void GetTiles(string directory, ObservableCollection<StartMenuTile> Tiles)
        {
            string folderPath = Environment.GetEnvironmentVariable("programdata") + @"\Startify\Tiles";

            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Tile folder created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating tiles folder: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Tiles folder already exists.");
            }
            foreach (string f in Directory.GetFiles(directory))
            {
                if (System.IO.Path.GetExtension(f) == ".lnk")
                {
                    Tiles.Add(new StartMenuTileLink()
                    {
                        TileName = System.IO.Path.GetFileNameWithoutExtension(f),
                        TileIcon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(f))),
                        Link = f,
                        Path = System.IO.Path.GetFullPath(f),
                    });
                }
            }
            GetTilesRecurse(Tiles, directory);
        }
        public void GetUWPTiles(ObservableCollection<StartMenuTile> Tiles)
        {
            // Create a PackageManager object
            PackageManager packageManager = new PackageManager();

            // Get the user SID
            string userSid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;

            // Get the packages for the current user
            var packages = packageManager.FindPackagesForUser(userSid);

            // Create a HashSet to store the added package names
            HashSet<string> addedPackages = new HashSet<string>();

            // Loop through the packages and add them to the Apps collection
            foreach (var package in packages)
            {
                if (package.IsFramework == false && package.IsResourcePackage == false && package.IsOptional == false && package.IsBundle == false)
                {
                    // Get the app list entries for the package
                    var appListEntries = package.GetAppListEntriesAsync().GetAwaiter().GetResult();

                    // Loop through the app list entries and find the default one
                    foreach (var appListEntry in appListEntries)
                    {
                        if (appListEntry != null)
                        {
                            string packageName = package.Id.Name;
                            {
                                // Check if the package name has already been added to the Apps collection
                                if (!addedPackages.Contains(packageName))
                                {
                                    // Add the package name to the HashSet
                                    addedPackages.Add(packageName);

                                    // Add the app entry to the Programs collection
                                    Tiles.Add(new StartMenuTileLink()
                                    {
                                        TileName = appListEntry.DisplayInfo.DisplayName,
                                        TileIcon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(package.Logo),
                                        PathUWP = package.Id.FamilyName
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }


        public void GetTilesRecurse(ObservableCollection<StartMenuTile> programs, string directory, StartMenuTileDirectory parent = null)
        {
            bool hasParent = parent != null;
            string[] subDirectories = Directory.GetDirectories(directory);

            foreach (string d in subDirectories)
            {
                DirectoryInfo dInfo = new DirectoryInfo(d);
                var a = dInfo.GetFiles("*.lnk", SearchOption.TopDirectoryOnly);
                var b = dInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

                if (a.Length > 0 || b.Length > 0) // Check if the directory is not empty
                {
                    StartMenuTileDirectory folderEntry = null;

                    if (!hasParent)
                    {
                        folderEntry = programs.FirstOrDefault(x => x.TileName == dInfo.Name) as StartMenuTileDirectory;
                    }

                    if (folderEntry == null)
                    {
                        folderEntry = new StartMenuTileDirectory
                        {
                            TileName = dInfo.Name,
                            Path = System.IO.Path.GetFullPath(d),
                            Links = new ObservableCollection<StartMenuTileLink>(),
                            Directories = new ObservableCollection<StartMenuTileDirectory>(),
                            Link = d,
                            TileIcon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/UnplatedFolder.png")),
                        };
                    }

                    var c = new string[a.Length + b.Length];
                    for (int i = 0; i < a.Length; i++)
                    {
                        c[i] = a[i].FullName;
                    }
                    for (int i = 0; i < b.Length; i++)
                    {
                        c[a.Length + i] = b[i].FullName;
                    }

                    foreach (var f in c)
                    {
                        folderEntry.HasChildren = true;
                        Uri uri = new Uri(uriString: IconHelper.GetFileIcon(f));
                        FileAttributes attr = System.IO.File.GetAttributes(f);
                        if (attr.HasFlag(FileAttributes.Directory))
                            uri = new Uri("ms-appx:///Assets/UnplatedFolder.png");

                        folderEntry.Links.Add(new StartMenuTileLink
                        {
                            TileName = System.IO.Path.GetFileNameWithoutExtension(f),
                            Link = f,
                            TileIcon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(uri),
                        });
                    }

                    if (!hasParent)
                    {
                        if (!programs.Contains(folderEntry))
                        {
                            programs.Add(folderEntry);
                        }
                    }
                    else
                    {
                        parent.Directories.Add(folderEntry);
                    }
                }
            }
        }
    }
}
