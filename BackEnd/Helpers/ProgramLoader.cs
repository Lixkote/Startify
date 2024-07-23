using ShellApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using WPF.Helpers;

namespace StartifyBackend.Helpers
{
    internal class ProgramLoader
    {
        private Size _logoSize;
        public void GetPrograms(string directory, ObservableCollection<StartMenuEntry> Programs)
        {
            foreach (string f in Directory.GetFiles(directory))
            {
                if (Path.GetExtension(f) == ".lnk")
                {
                    Programs.Add(new StartMenuLink
                    {
                        Title = Path.GetFileNameWithoutExtension(f),
                        Link = f,
                        Path = Path.GetFullPath(f),
                        Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(f)))
                    });
                }
            }
            GetProgramsRecurse(Programs, directory);
        }
        public async void GetUWPApps(ObservableCollection<StartMenuEntry> Programs)
        {
            // Create a PackageManager object
            PackageManager packageManager = new PackageManager();

            // Get the user SID
            string userSid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;

            // Get the packages for the current user
            var packages = packageManager.FindPackagesForUser("");

            // Create a HashSet to store the added package names
            HashSet<string> addedPackages = new HashSet<string>();

            RandomAccessStreamReference logoData;

            // Loop through the packages and add them to the Apps collection
            foreach (var package in packages)
            {
                if (!package.IsFramework && !package.IsResourcePackage && !package.IsStub && package.GetAppListEntries().FirstOrDefault() != null)
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

                                    // Create a StartMenuLink instance without the Icon initially
                                    var startMenuLink = new StartMenuLink()
                                    {
                                        Title = appListEntry.DisplayInfo.DisplayName,
                                        PathUWP = package.Id.FamilyName
                                    };

                                    try
                                    {
                                        _logoSize = new Size(176, 176);
                                        IReadOnlyList<AppListEntry> entries = package.GetAppListEntries();
                                        foreach (AppListEntry entry in entries)
                                        {

                                            logoData = null;
                                            logoData = package.GetLogoAsRandomAccessStreamReference(_logoSize);

                                            IRandomAccessStreamWithContentType stream = await logoData.OpenReadAsync();
                                            BitmapImage bitmapImage = new BitmapImage();
                                            await bitmapImage.SetSourceAsync(stream);
                                            startMenuLink.Icon = bitmapImage;
                                        }
                                    }
                                    catch
                                    {
                                        // If either check fails, you can handle it here or set a default icon
                                        // For example:
                                        startMenuLink.Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/UnplatedFolder.png"));
                                    }

                                    // Add the StartMenuLink to Programs
                                    Programs.Add(startMenuLink);



                                }
                            }
                        }
                    }
                }
            }
        }


        public void GetProgramsRecurse(ObservableCollection<StartMenuEntry> programs, string directory, StartMenuDirectory parent = null)
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
                    StartMenuDirectory folderEntry = null;

                    if (!hasParent)
                    {
                        folderEntry = programs.FirstOrDefault(x => x.Title == dInfo.Name) as StartMenuDirectory;
                    }

                    if (folderEntry == null)
                    {
                        folderEntry = new StartMenuDirectory
                        {
                            Title = dInfo.Name,
                            Path = Path.GetFullPath(d),
                            Links = new ObservableCollection<StartMenuLink>(),
                            Directories = new ObservableCollection<StartMenuDirectory>(),
                            Link = d,
                            Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/UnplatedFolder.png")),
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
                        FileAttributes attr = File.GetAttributes(f);
                        if (attr.HasFlag(FileAttributes.Directory))
                            uri = new Uri("ms-appx:///Assets/UnplatedFolder.png");

                        folderEntry.Links.Add(new StartMenuLink
                        {
                            Title = Path.GetFileNameWithoutExtension(f),
                            Link = f,
                            Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(uri),
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
