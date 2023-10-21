using IWshRuntimeLibrary;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Wpf.UI.XamlHost;
using Microsoft.Win32;
using Shell.Shell.Start.Tiles;
using ShellApp;
using ShellApp.Shell.Start;
using StartifyBackend.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Xml;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WPF.Helpers;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WPF.Views
{
    /// <summary>
    /// Interaction logic for StartMenu.xaml
    /// </summary>
    public partial class StartMenu : System.Windows.Window
    {
        Helpers.StartMenuListener StartListener;
        Helpers.RegistryMonitor AccentListener;
        Helpers.RegistryMonitor ThemeListener;
        Helpers.ProgramsApps.LaunchAppProgram AppLauncher = new Helpers.ProgramsApps.LaunchAppProgram();
        Helpers.ProgramsApps.ProgramGetHelper AppHelper = new Helpers.ProgramsApps.ProgramGetHelper();
        TilesLoadHelper TileAppHelper = new Helpers.TilesLoadHelper();
        Helpers.Launching.Startup startup = new Helpers.Launching.Startup();
        Helpers.StartMenuTools startMenuTools = new Helpers.StartMenuTools();
        public bool applistwasloaded = false;
        private IntPtr HKEY_CURRENT_USER;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        const uint EWX_LOGOFF = 0x00000000;

        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<TileGroup> TileGroups = new ObservableCollection<TileGroup>();
        public StartMenu()
        {
            // Initialize Startify
            InitializeComponent();
            StartListener = new StartMenuListener();

            AccentListener = new RegistryMonitor(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            AccentListener.RegChanged += new EventHandler(ApplyNewAccent);
            AccentListener.Start();

            ThemeListener = new RegistryMonitor(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            ThemeListener.RegChanged += new EventHandler(ApplyNewShellTheme);
            ThemeListener.Start();

            StartListener.StartTriggered += OnStartTriggered;
            StartListener.ListenerErrorHappened += StartifyErrorOccured;
            TileAppHelper.CouldNotLoadTiles += DisableTiles;
            StartListener.FindAndActivateWindow();
            startMenuTools.AlignStartifyWithTaskbar(this);
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            // Do this so the app wont wait for user start button press on startup.
            Show();
            Hide();
            Logger.LogError("very example test error");
        }

        private void DisableTiles(object sender, EventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            var StartBackground = startPlaceholder.FindName("startbackground") as Windows.UI.Xaml.Controls.Grid;
            StartBackground.Width = 300;
            ShowStolenTiles();
        }

        private void ThemingSetup()
        {
            try
            {
                int themeValue2 = (int)Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize").GetValue("SystemUsesLightTheme");

                // Use the Dispatcher to run the UI-related code on the UI thread
                Dispatcher.Invoke(() =>
                {
                    var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;

                    if (themeValue2 == 1)
                    {
                        startPlaceholder.RequestedTheme = Windows.UI.Xaml.ElementTheme.Light;
                    }
                    else if (themeValue2 == 0)
                    {
                        startPlaceholder.RequestedTheme = Windows.UI.Xaml.ElementTheme.Dark;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Apply shell theme failed: " + ex.ToString());
            }
        }

        private void ApplyNewAccent(object sender, EventArgs e)
        {
            int themeValue = (int)Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize").GetValue("ColorPrevalence");
            if (themeValue == 1)
            {
                var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
                startPlaceholder.AccentWasEnabled();
            }
            else if (themeValue == 0)
            {
                var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
                startPlaceholder.AccentWasDisabled();
            }
        }
        private void ApplyNewShellTheme(object sender, EventArgs e)
        {
            try
            {
                int themeValue2 = (int)Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize").GetValue("SystemUsesLightTheme");

                // Use the Dispatcher to run the UI-related code on the UI thread
                Dispatcher.Invoke(() =>
                {
                    var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;

                    if (themeValue2 == 1)
                    {
                        startPlaceholder.RequestedTheme = Windows.UI.Xaml.ElementTheme.Light;
                    }
                    else if (themeValue2 == 0)
                    {
                        startPlaceholder.RequestedTheme = Windows.UI.Xaml.ElementTheme.Dark;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Apply shell theme failed: " + ex.ToString());
            }
        }


        private void StartifyErrorOccured(object sender, EventArgs e)
        {
            ShowErrorNotification();
        }


        public async void ShowStolenTiles()
        {
            // Get the path to the "Assets" folder in the current running directory
            string assetsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

            // Specify the file name you want to access
            string fileName = "error.png";

            // Combine the folder path with the file name to get the full file path
            string filePath = Path.Combine(assetsFolderPath, fileName);

            // Create a URI object from the file path
            Uri fileUri = new Uri(filePath);

            // Get the URI as a string
            string uriString = fileUri.ToString();

            // Show the "everything is ok" toast
            new ToastContentBuilder()
                .AddInlineImage(new Uri(uriString))
                .AddText("Tiles functionality disabled.")
                .AddText("We could not find or load the tiles configuration file.")
                .Show();
        }


        public async void ShowNotification()
        {
            // Get the path to the "Assets" folder in the current running directory
            string assetsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

            // Specify the file name you want to access
            string fileName = "halal.png";

            // Combine the folder path with the file name to get the full file path
            string filePath = Path.Combine(assetsFolderPath, fileName);

            // Create a URI object from the file path
            Uri fileUri = new Uri(filePath);

            // Get the URI as a string
            string uriString = fileUri.ToString();

            // Show the "everything is ok" toast
            new ToastContentBuilder()
                .AddInlineImage(new Uri(uriString))
                .AddText("Startify launched")
                .AddText("Try clicking the Windows Start button!")
                .Show(); 
        }
        public void ShowErrorNotification()
        {
            // Get the path to the "Assets" folder in the current running directory
            string assetsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

            // Specify the file name you want to access
            string fileName = "error.png";

            // Combine the folder path with the file name to get the full file path
            string filePath = Path.Combine(assetsFolderPath, fileName);

            // Create a URI object from the file path
            Uri fileUri = new Uri(filePath);

            // Get the URI as a string
            string uriString = fileUri.ToString();

            // Show the "everything is ok" toast
            new ToastContentBuilder()
                .AddInlineImage(new Uri(uriString))
                .AddText(":(")
                .AddText("Startify encountered an internal error and may not work properly.")
                .Show();
        }

        public async Task ShowByeNotification()
        {
            // Get the path to the "Assets" folder in the current running directory
            string assetsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");

            // Specify the file name you want to access
            string fileName = "bye.png";

            // Combine the folder path with the file name to get the full file path
            string filePath = Path.Combine(assetsFolderPath, fileName);

            // Create a URI object from the file path
            Uri fileUri = new Uri(filePath);

            // Get the URI as a string
            string uriString = fileUri.ToString();

            // Show the "everything is ok" toast
            new ToastContentBuilder()
                .AddInlineImage(new Uri(uriString))
                .AddText("Startify disabled")
                .AddText("See you later")
                .Show();
        }

        async void OnStartTriggered(object sender, EventArgs e)
        {
            var newVisibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

            if (newVisibility == Visibility.Hidden)
            {
                var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
                Task animationTask = startPlaceholder.StartCloseStartAnimation();
                await animationTask; // Wait for the animation task to finish
                Hide(); // Hide the window
            }

            Visibility = newVisibility;

            if (Visibility == Visibility.Visible)
            {
                Show();               
                WindowActivator.ActivateWindow(new System.Windows.Interop.WindowInteropHelper(startmenubasewindow).Handle);
                RefreshAppList();
                this.Focus();
            }
        }


        public void RefreshAppList()
        {
            if (applistwasloaded == true) 
            {
                var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
                var allAppsListView = startPlaceholder.FindName("AllAppsListView") as Windows.UI.Xaml.Controls.ListView;
                if (allAppsListView.Items.Count > 0)
                {
                    var firstItem = allAppsListView.Items[0]; // Get the first item in your data source
                    allAppsListView.ScrollIntoView(firstItem);
                }
                var cvs = startPlaceholder.FindName("cvs") as Windows.UI.Xaml.Data.CollectionViewSource;
                cvs.Source = from p in Programs
                             orderby p.Alph
                             group p by char.IsDigit(p.Alph[0]) ? "#" : p.Alph into g
                             select g;
            }
        }




        private async void StartMenuActivated(object sender, EventArgs e)
        {
            Show();
            startMenuTools.AlignStartifyWithTaskbar(this);
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            Task animationTask = startPlaceholder.StartOpenStartAnimation();
            await animationTask; // wait for the animation task to finish
            this.Focus();
        }

        private async void StartMenuDeactivated(object sender, EventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            Task animationTask = startPlaceholder.StartCloseStartAnimation();
            await animationTask; // wait for the animation task to finish
            Hide();
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


        private async Task AccountSettingsAsync()
        {
            Process.Start(new ProcessStartInfo("ms-settings:yourinfo") { UseShellExecute = true });
        }
        private async Task SignoutAsync()
        {
            // Exit the WPF application
            System.Windows.Application.Current.Shutdown();
            // Log off the current user
            ExitWindowsEx(EWX_LOGOFF, 0);
        }
        private async Task LockAsync()
        {
            // Define some constants
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;

            // Get the virtual key codes for Windows and L keys
            byte winKey = (byte)KeyInterop.VirtualKeyFromKey(Key.LWin);
            byte lKey = (byte)KeyInterop.VirtualKeyFromKey(Key.L);

            // Press the Windows key
            keybd_event(winKey, MapVirtualKey(winKey, 0), KEYEVENTF_EXTENDEDKEY, 0);

            // Press the L key
            keybd_event(lKey, MapVirtualKey(lKey, 0), KEYEVENTF_EXTENDEDKEY, 0);

            // Release the L key
            keybd_event(lKey, MapVirtualKey(lKey, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            // Release the Windows key
            keybd_event(winKey, MapVirtualKey(winKey, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        private async Task CloseStartify()
        {
            await ShowByeNotification();
            System.Windows.Application.Current.Shutdown();
        }

        private async Task OpenUninstall()
        {
            Process.Start(new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true });
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

        private void StartMenuIsland_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach all of the xaml functions and actions to the WPF part of the app.

            string commonStartMenu = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            string userStartMenu = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");

            AppHelper.GetPrograms(commonStartMenu, Programs);
            AppHelper.GetPrograms(userStartMenu, Programs);

            Programs = new ObservableCollection<StartMenuEntry>(Programs.OrderBy(x => x.Title));

            AppHelper.GetUWPApps(Programs);

            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            var allAppsListView = startPlaceholder.FindName("AllAppsListView") as Windows.UI.Xaml.Controls.ListView;

            allAppsListView.ItemClick += (sender, e) => AppLauncher.AppLaunchHandler(sender, e, this, Programs, false);

            var cvs = startPlaceholder.FindName("cvs") as Windows.UI.Xaml.Data.CollectionViewSource;
            cvs.Source = from p in Programs
                         orderby p.Alph
                         group p by char.IsDigit(p.Alph[0]) ? "#" : p.Alph into g
                         select g;
            allAppsListView.Loaded += applistloaded;

            var hibernate = startPlaceholder.FindName("HibernateMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var sleep = startPlaceholder.FindName("SleepMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var restart = startPlaceholder.FindName("RestartMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var power = startPlaceholder.FindName("PowerMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var exitmenuitem = startPlaceholder.FindName("ExitStartify") as Windows.UI.Xaml.Controls.MenuFlyoutItem;

            var signout = startPlaceholder.FindName("SignOutMenuItem") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var lockout = startPlaceholder.FindName("LockMenuItem") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var accsetting = startPlaceholder.FindName("AccountSettingsMenuItem") as Windows.UI.Xaml.Controls.MenuFlyoutItem;

            var powerKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Power");
            int hibernateValue = (int)powerKey.GetValue("HibernateEnabledDefault");
            hibernate.Visibility = (hibernateValue == 1) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;

            hibernate.Click += async (sender, e) => await HibernateAsync();
            sleep.Click += async (sender, e) => await SleepAsync();
            power.Click += async (sender, e) => await ShutdownAsync();
            restart.Click += async (sender, e) => await RestartAsync();

            signout.Click += async (sender, e) => await SignoutAsync();
            lockout.Click += async (sender, e) => await LockAsync();
            accsetting.Click += async (sender, e) => await AccountSettingsAsync();

            exitmenuitem.Click += async (sender, e) => await CloseStartify();

            // startPlaceholder.OpenFileLocationClicked += async (sender, e) => await OpenUninstall();
            // startPlaceholder.RunAsAdminClicked += async (sender, e) => await AppLauncher.DirectoryAppLaunchHandler(sender, e, this, Programs, false);
            startPlaceholder.UninstallSettingsShouldOpen += async (sender, e) => await OpenUninstall();
            ShowNotification();
            LoadTiles();
            ThemingSetup();
        }

        public void LoadTiles()
        {
            TileGroups = new ObservableCollection<TileGroup>();
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            TileAppHelper.LoadTileGroups(TileGroups);

            var tilegridview = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            tilegridview.ItemsSource = TileGroups;
        }

        private void applistloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            // Find the ListView element by its name
            startPlaceholder.DirectoryChildClicked += (sender, e) => AppLauncher.DirectoryAppLaunchHandler(sender, e, this, Programs, false);
            applistwasloaded = true;
        }
        private async void startmenubasewindow_LostFocus(object sender, RoutedEventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            Task animationTask = startPlaceholder.StartCloseStartAnimation();
            await animationTask; // wait for the animation task to finish
            Hide();
        }

        // Declare the Win32 API functions
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern byte MapVirtualKey(byte wCode, int wMap);

        private void LaunchWindowsSearch(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Check if the pressed key is a letter (A-Z)
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                // Define some constants
                const int KEYEVENTF_EXTENDEDKEY = 0x1;
                const int KEYEVENTF_KEYUP = 0x2;

                // Get the virtual key codes for Windows and S keys
                byte winKey = (byte)KeyInterop.VirtualKeyFromKey(Key.LWin);
                byte sKey = (byte)KeyInterop.VirtualKeyFromKey(Key.S);

                // Press the Windows key
                keybd_event(winKey, MapVirtualKey(winKey, 0), KEYEVENTF_EXTENDEDKEY, 0);

                // Press the S key
                keybd_event(sKey, MapVirtualKey(sKey, 0), KEYEVENTF_EXTENDEDKEY, 0);

                // Release the S key
                keybd_event(sKey, MapVirtualKey(sKey, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

                // Release the Windows key
                keybd_event(winKey, MapVirtualKey(winKey, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }
        }
    }
}
