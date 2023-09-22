using IWshRuntimeLibrary;
using Microsoft.Toolkit.Wpf.UI.XamlHost;
using Microsoft.Win32;
using ShellApp;
using ShellApp.Shell.Start;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WPF.Helpers;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WPF.Views
{
    /// <summary>
    /// Interaction logic for StartMenu.xaml
    /// </summary>
    public partial class StartMenu : Window
    {
        Helpers.StartMenuListener _listener;
        public StartMenu()
        {
            InitializeComponent();
            _listener = new StartMenuListener();
            _listener.StartTriggered += OnStartTriggered;
            _listener.FindAndActivateWindow();
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = 0;
            Top = desktopWorkingArea.Bottom - Height;
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            this.Focus();
        }

        void OnStartTriggered(object sender, EventArgs e)
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            if (Visibility == Visibility.Visible)
            {
                Show();
                WindowActivator.ActivateWindow(new System.Windows.Interop.WindowInteropHelper(startmenubasewindow).Handle);
            }
        }
        private void StartMenuActivated(object sender, EventArgs e)
        {
            Screen screen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
            this.Left = screen.WorkingArea.Left;
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            startPlaceholder.StartOpenStartAnimation();
        }

        private void StartMenuDeactivated(object sender, EventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            startPlaceholder.StartCloseStartAnimation();
            Visibility = Visibility.Hidden;
            Hide();
        }

        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<StartMenuLink> Links = new ObservableCollection<StartMenuLink>();
        ObservableCollection<StartMenuLink> Results = new ObservableCollection<StartMenuLink>();



        private void StartMenuIsland_Loaded(object sender, RoutedEventArgs e)
        {
            // this is responsible for filling the xaml listview with programs and folders
            string programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            GetPrograms(programs);
            programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            GetPrograms(programs);
            Programs = new ObservableCollection<StartMenuEntry>(Programs.OrderBy(x => x.Title));


            //Get the installed UWP apps and display them on the xaml App list
            GetUWPApps();

            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;

            // Find the ListView element by its name
            var allAppsListView = startPlaceholder.FindName("AllAppsListView") as Windows.UI.Xaml.Controls.ListView;

            // Find the smaller ListView element by its name
            var allAppsListViewFolder = startPlaceholder.FindName("DirectoryChildContainer") as Windows.UI.Xaml.Controls.ListView;

            // Find the CollectionViewSource element by its name
            var cvs = startPlaceholder.FindName("cvs") as Windows.UI.Xaml.Data.CollectionViewSource;

            // Set the ItemsSource property of the ListView element to Programs
            // allAppsListView.ItemsSource = Programs;
            // Assign it to the ItemClick property of your UWP ListView control
            allAppsListView.ItemClick += launchhandler;

            var groups = from p in Programs
                         orderby p.Alph
                         group p by char.IsDigit(p.Alph[0]) ? "#" : p.Alph into g
                         select g;

            cvs.Source = groups;

            allAppsListView.Loaded += applistloaded;


            //////////////////////////////////////////////////////////////////////////////////
            /// Set power button actions and hibernate item visibility.
            //////////////////////////////////////////////////////////////////////////////////

            // Find the buttons by its name
            var hibernate = startPlaceholder.FindName("HibernateMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var sleep = startPlaceholder.FindName("SleepMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var restart = startPlaceholder.FindName("RestartMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var power = startPlaceholder.FindName("PowerMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;

            // Determine if Hibernate button should be shown.

            // Open the registry key for power settings
            RegistryKey powerKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Power");

            // Get the value of HibernateEnabled
            int hibernateValue = (int)powerKey.GetValue("HibernateEnabledDefault");

            // very epic check 
            if (hibernateValue == 1)
            {
                hibernate.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                hibernate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            hibernate.Click += async (sender, e) => await HibernateAsync();
            sleep.Click += async (sender, e) => await SleepAsync();
            power.Click += async (sender, e) => await ShutdownAsync();
            restart.Click += async (sender, e) => await RestartAsync();


            //Colorization dishery
            var colorization = startPlaceholder.FindName("IsColorizationEnabled") as Windows.UI.Xaml.Controls.TextBlock;
            RegistryKey themekey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");

            int themevalue = (int)themekey.GetValue("ColorPrevalence");
            colorization.Text = themevalue.ToString();
        }

        private void applistloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                // Get the StartPlaceholder object from the WindowsXamlHost element
                var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
                // Find the ListView element by its name
                startPlaceholder.DirectoryChildClicked += launchhandleralt;
            }
            catch (Exception ex) 
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }

        private async void launchhandleralt(object sender, ItemClickEventArgs e)
        {
            StartMenuLink clickedItem = e.ClickedItem as StartMenuLink;
            // Get the index of the clicked item in the ObservableCollection
            int index = Links.IndexOf(clickedItem);

            System.Windows.MessageBox.Show("Not yet implemented :/");
            
        }
        private async void launchhandler(object sender, ItemClickEventArgs e)
        {
            StartMenuEntry clickedItem = e.ClickedItem as StartMenuEntry;
            // Get the index of the clicked item in the ObservableCollection
            int index = Programs.IndexOf(clickedItem);

            // Get the path of the clicked item from the ObservableCollection
            string path = Programs[index].Path;
            string pathuwp = Programs[index].PathUWP;

            // Do something with the index and path
            this.Hide();
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
        static async Task<AppListEntry> GetAppByPackageFamilyNameAsync(string packageFamilyName)
        {
            var pkgManager = new PackageManager();
            var pkg = pkgManager.FindPackagesForUser("", packageFamilyName).FirstOrDefault();

            if (pkg == null) return null;

            var apps = await pkg.GetAppListEntriesAsync();
            var firstApp = apps.FirstOrDefault();
            return firstApp;
        }


        private async Task HibernateAsync()
        {
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Hibernate, true, true);
        }

        private async Task SleepAsync()
        {
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, true, true);
        }
        private async Task ShutdownAsync()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
        }
        private async Task RestartAsync()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }

        public static bool IsHibernateEnabled()
        {
            // Open the registry key for power settings
            RegistryKey powerKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Power");

            // Get the value of HibernateEnabled
            int hibernateValue = (int)powerKey.GetValue("HibernateEnabled");

            // Return true if HibernateEnabled is 1, false otherwise
            return hibernateValue == 1;
        }


        private void GetPrograms(string directory)
        {
            foreach (string f in Directory.GetFiles(directory))
            {
                if (System.IO.Path.GetExtension(f) != ".ini")
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
            GetProgramsRecurse(directory);
        }



    private void GetUWPApps()
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


        private void GetProgramsRecurse(string directory, StartMenuDirectory parent = null)
        {
            bool hasParent = parent != null;
            foreach (string d in Directory.GetDirectories(directory))
            {
                StartMenuDirectory folderEntry = null;
                if (!hasParent)
                {
                    folderEntry = Programs.FirstOrDefault(x => x.Title == new DirectoryInfo(d).Name) as StartMenuDirectory;
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

                GetProgramsRecurse(d, folderEntry);
                foreach (string f in Directory.GetFiles(d))
                {
                    folderEntry.HasChildren = true;
                    if (System.IO.Path.GetExtension(f) != ".ini")
                    {
                        folderEntry.Links.Add(new StartMenuLink
                        {
                            Title = System.IO.Path.GetFileNameWithoutExtension(f),
                            Link = f,
                            Icon = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(uriString: IconHelper.GetFileIcon(f)))
                        });
                    }
                }

                if (!hasParent)
                {
                    if (!Programs.Contains(folderEntry))
                    {
                        Programs.Add(folderEntry);
                    }
                }
                else
                {
                    parent.Directories.Add(folderEntry);
                }
            }
        }

        private void startmenubasewindow_LostFocus(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
