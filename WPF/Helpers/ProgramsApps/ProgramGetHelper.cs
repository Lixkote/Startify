using ShellApp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Management.Deployment;

namespace WPF.Helpers.ProgramsApps
{
    internal class ProgramGetHelper
    {
        public void GetPrograms(string directory, ObservableCollection<StartMenuEntry> Programs)
        {
            foreach (string f in Directory.GetFiles(directory))
            {
                if (System.IO.Path.GetExtension(f) == ".lnk")
                {
                    Programs.Add(new StartMenuLink
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(f),
                        Link = f,
                        Path = System.IO.Path.GetFullPath(f),
                        Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(f)))
                    });
                }
            }
            GetProgramsRecurse(Programs, directory);
        }
        public void GetUWPApps(ObservableCollection<StartMenuEntry> Programs)
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
                                    Programs.Add(new StartMenuLink()
                                    {
                                        Title = appListEntry.DisplayInfo.DisplayName,
                                        Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(package.Logo),
                                        PathUWP = package.Id.FamilyName
                                    });
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
            foreach (string d in Directory.GetDirectories(directory))
            {
                StartMenuDirectory folderEntry = null;
                if (!hasParent)
                {
                    folderEntry = programs.FirstOrDefault(x => x.Title == new DirectoryInfo(d).Name) as StartMenuDirectory;
                }
                if (folderEntry == null)
                {
                    folderEntry = new StartMenuDirectory
                    {
                        Title = new DirectoryInfo(d).Name,
                        Path = System.IO.Path.GetFullPath(d),
                        Links = new ObservableCollection<StartMenuLink>(),
                        Directories = new ObservableCollection<StartMenuDirectory>(),
                        Link = d,
                        Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/UnplatedFolder.png"))
                    };
                }
                DirectoryInfo dInfo = new DirectoryInfo(d);
                var a = dInfo.GetFiles("*.lnk", SearchOption.TopDirectoryOnly);
                var b = dInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
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

                    folderEntry.Links.Add(new StartMenuLink
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(f),
                        Link = f,
                        Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(uri)
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
