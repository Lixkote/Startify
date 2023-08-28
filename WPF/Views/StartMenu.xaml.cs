using IWshRuntimeLibrary;
using Microsoft.Toolkit.Wpf.UI.XamlHost;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = 0;
            Top = desktopWorkingArea.Bottom - Height;
        }

        void OnStartTriggered(object sender, EventArgs e)
        {

            Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            if (Visibility == Visibility.Visible)
            {
                Show();
                //Storyboard closestartanim = (Storyboard)FindResource("openstartanim");
                //closestartanim.Begin();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Screen screen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
            this.Left = screen.WorkingArea.Left;
        }

        private void Menu_Deactivated(object sender, EventArgs e)
        {
            // Get the storyboard from the resources
            //Storyboard closestartanim = (Storyboard)FindResource("closestartanim");
            //closestartanim.Begin();
            Visibility = Visibility.Hidden;
            Hide();
        }

        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<StartMenuLink> Results = new ObservableCollection<StartMenuLink>();

        private void StartMenuIsland_Loaded(object sender, RoutedEventArgs e)
        {
            // this is responsible for filling the xaml listview with programs and folders
            string programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            GetPrograms(programs);
            programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            GetPrograms(programs);
            Programs = new ObservableCollection<StartMenuEntry>(Programs.OrderBy(x => x.Title));

            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;

            // Find the ListView element by its name
            var allAppsListView = startPlaceholder.FindName("AllAppsListView") as Windows.UI.Xaml.Controls.ListView;

            // Set the ItemsSource property of the ListView element to Programs
            allAppsListView.ItemsSource = Programs;
            // Assign it to the ItemClick property of your UWP ListView control
            allAppsListView.ItemClick += launchhandler;


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




        }

        private void launchhandler(object sender, ItemClickEventArgs e)
        {
            StartMenuEntry clickedItem = e.ClickedItem as StartMenuEntry;
            // Get the index of the clicked item in the ObservableCollection
            int index = Programs.IndexOf(clickedItem);

            // Get the path of the clicked item from the ObservableCollection
            string path = Programs[index].Path;

            // Do something with the index and path
            this.Hide();
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
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



    }
}
