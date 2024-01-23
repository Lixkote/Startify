using IWshRuntimeLibrary;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Wpf.UI.XamlHost;
using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using Shell.Interface.StartMenu;
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
using System.Text.RegularExpressions;
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
using File = System.IO.File;
using MessageBox = ModernWpf.MessageBox;
using Tile = WPF.Helpers.Tile;
using Window = System.Windows.Window;

namespace WPF.Views
{
    /// <summary>
    /// Interaction logic for StartMenu.xaml
    /// </summary>
    public partial class StartMenu : System.Windows.Window
    {
        // Mandatory definitions under here
        TilesManager TileAppHelper = new Helpers.TilesManager();
        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<TileGroup> TileGroups = new ObservableCollection<TileGroup>();
        ProgramLoader AppHelper = new ProgramLoader();
        AppLauncher Launcher = new AppLauncher();
        bool applistwasloaded = false;
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        static extern byte MapVirtualKey(byte wCode, int wMap);
        App Engine = App.Current as App;

        ////////////////////////////////////////////////////////////////////////////
        // Custom Items under this comment, specific for this start menu type
        // So like, this start menu uses tiles, so here will be the tile specific stuff
        ////////////////////////////////////////////////////////////////////////////
        
        private void DisableTiles(object sender, EventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            var StartBackground = startPlaceholder.FindName("startbackground") as Windows.UI.Xaml.Controls.Grid;
            StartBackground.Width = 300;
            Engine.ShowStolenTiles();

        }

        public void LoadTiles()
        {
            TileGroups = new ObservableCollection<TileGroup>();
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            TileAppHelper.LoadTileGroups(TileGroups);

            var tilegridview = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            tilegridview.ItemsSource = TileGroups;
        }

        public void ReloadTiles()
        {
            TileGroups = new ObservableCollection<TileGroup>();
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            TileAppHelper.LoadTileGroups(TileGroups);

            var tilegridview = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            tilegridview.ItemsSource = null;
            tilegridview.ItemsSource = TileGroups;
        }

        private void TilesLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            // Find the ListView element by its name
            startPlaceholder.TileClickedMain += (sender, e) => TileAppHelper.OpenTileApp(sender, e, this, false);
            startPlaceholder.TileUnpinnedMain += (sender, e) => UnpinTile(sender, e);
            startPlaceholder.TilePinnedMain += (sender, e) => PinTile(sender, e);
        }

        private Tile FindTileByPathClassic(string pathClassicValue)
        {
            foreach (var group in TileGroups)
            {
                var tile = group.Tiles.FirstOrDefault(t => String.Equals(t.PathClassic, pathClassicValue, StringComparison.OrdinalIgnoreCase));
                if (tile != null)
                {
                    return tile;
                }
            }

            return null; // Return null if the tile is not found
        }

        public void UnpinTile(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Debug statement to check if the method is being called
            Debug.WriteLine("UnpinTile method called.");

            // Remove the tile from the XML configuration file
            TileAppHelper.RemoveTileFromXml(sender);
            // Your input string
            string input = sender as string;

            // Define the pattern for extracting paths
            string pattern = @"TilePathClassic:(.*?)TileGroupName:(.*?)$";

            // Use Regex to match the pattern
            Match match = Regex.Match(input, pattern);


            // Check if the match was successful
            if (match.Success)
            {

                // Extract the PathClassic value from the string
                string pathClassicValue = match.Groups[1].Value.Trim();

                // Find the tile with the same PathClassic value
                var tileToRemove = FindTileByPathClassic(pathClassicValue);

                if (tileToRemove != null)
                {
                    // Remove the tile from the XML configuration file
                    TileAppHelper.RemoveTileFromXml(tileToRemove);

                    // Update the TileGroups collection
                    var pinnedGroup = TileGroups.FirstOrDefault(group => group.Name == match.Groups[2].Value.Trim());
                    if (pinnedGroup != null)
                    {
                        pinnedGroup.Tiles.Remove(tileToRemove);
                    }
                }
                else
                {
                    Debug.WriteLine($"Tile with PathClassic value '{pathClassicValue}' not found.");
                }

                // Refresh the GridView for the specific group
                var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
                var tileGroupGridView = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;

                // Reset the ItemsSource to trigger a refresh
                tileGroupGridView.ItemsSource = null;
                tileGroupGridView.ItemsSource = TileGroups;

                // Debug statement to check if ItemsSource is set again
                Debug.WriteLine("TileGroupGridView refreshed.");

                // Debug statement to check the status of TileGroups after removal
                Debug.WriteLine($"TileGroups count after removal: {TileGroups.Count}");
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

        public void PinTile(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Debug statement to check if the method is being called
            Debug.WriteLine("PinTile method called.");

            // Remove the tile from the XML configuration file
            TileAppHelper.AddTileToXml(sender, TileGroups);
            // Refresh the GridView for the specific group
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            var tileGroupGridView = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            tileGroupGridView.ItemsSource = null;
            tileGroupGridView.ItemsSource = TileGroups;
        }

        private void applistloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            // Find the ListView element by its name
            startPlaceholder.DirectoryChildClicked += (sender, e) => Launcher.DirectoryAppLaunchHandler(sender, e, this, Programs, false);
            applistwasloaded = true;
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
            var TileGroupGridView = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;

            allAppsListView.ItemClick += (sender, e) => Launcher.AppLaunchHandler(sender, e, this, Programs, false);

            var cvs = startPlaceholder.FindName("cvs") as Windows.UI.Xaml.Data.CollectionViewSource;
            cvs.Source = from p in Programs
                         orderby p.Alph
                         group p by char.IsDigit(p.Alph[0]) ? "#" : p.Alph into g
                         select g;
            allAppsListView.Loaded += applistloaded;
            TileGroupGridView.Loaded += TilesLoaded;

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

            hibernate.Click += async (sender, e) => await Engine.HibernateAsync();
            sleep.Click += async (sender, e) => await Engine.SleepAsync();
            power.Click += async (sender, e) => await Engine.ShutdownAsync();
            restart.Click += async (sender, e) => await Engine.RestartAsync();

            signout.Click += async (sender, e) => await Engine.SignoutAsync();
            lockout.Click += async (sender, e) => await Engine.LockAsync();
            accsetting.Click += async (sender, e) => await Engine.AccountSettingsAsync();

            exitmenuitem.Click += async (sender, e) => await CloseStartify();

            // startPlaceholder.OpenFileLocationClicked += async (sender, e) => await OpenUninstall();
            // startPlaceholder.RunAsAdminClicked += async (sender, e) => await AppLauncher.DirectoryAppLaunchHandler(sender, e, this, Programs, false);
            startPlaceholder.UninstallSettingsShouldOpen += async (sender, e) => await OpenUninstall();
            LoadTiles();
            ThemingSetup();
            Engine.ShowNotification();
        }

        private async Task OpenUninstall()
        {
            Process.Start(new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true });
        }

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

        /////////////////////////////////////////////////////////////
        // Default or mandatory statify functions under this comment.
        // Stuff like aligning with the taskbar and so on
        // If you'll be adding a new start menu layout, you can copy these to the new layout.
        ////////////////////////////////////////////////////////////

        public StartMenu()
        {
            // Initialize Normal Tiled Start Menu Layout
            InitializeComponent();

            // Connect to the StartifyEngine
            Engine.FailedToLoadTiles += DisableTiles;
            Engine.ApplyNewAccent += ApplyNewAccent;
            Engine.ApplyNewShellTheme += ApplyNewShellTheme;
            Engine.StartTrigger += OnStartTriggered;

            // Mandatory alignment stuff and so on
            Show();
            Hide();
            AlignStartifyWithTaskbar(this);
        }

        public void AlignStartifyWithTaskbar(Window window)
        {
            try
            {
                //Align the start menu with taskbar alignment (center or left)
                RegistryKey alignkey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced");

                int alignvalue = (int)alignkey.GetValue("TaskbarAl");

                if (alignvalue.ToString() == "0")
                {
                    var desktopWorkingArea = SystemParameters.WorkArea;
                    window.Left = 0;
                    window.Top = desktopWorkingArea.Bottom - window.Height;
                }
                else if (alignvalue.ToString() == "1")
                {
                    // Calculate the screen center coordinates
                    double screenWidth = SystemParameters.PrimaryScreenWidth;
                    double screenHeight = SystemParameters.PrimaryScreenHeight;
                    var desktopWorkingArea = SystemParameters.WorkArea;
                    double windowWidth = window.Width;
                    double windowHeight = window.Height;

                    double left = (screenWidth - windowWidth) / 2;

                    // Set the window position to the center of the screen
                    window.Left = left;
                    window.Top = desktopWorkingArea.Bottom - window.Height;
                }
            }
            catch (Exception ex)
            {
                ModernWpf.MessageBox.Show("Startify has issues reading the taskbar alignment registry key. The default(left) alignment will be used. Error code: " + ex.ToString(), "Startify Backend Error", MessageBoxButton.OK, SymbolGlyph.Error, MessageBoxResult.OK);
            }
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
                ModernWpf.MessageBox.Show("Startify has issues applying the shell theme." + ex.ToString(), "Startify Backend Error", MessageBoxButton.OK, SymbolGlyph.Error, MessageBoxResult.OK);
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
                ModernWpf.MessageBox.Show("Startify has issues applying the shell theme(second method)." + ex.ToString(), "Startify Backend Error", MessageBoxButton.OK, SymbolGlyph.Error, MessageBoxResult.OK);
            }
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

        private async void StartMenuActivated(object sender, EventArgs e)
        {
            Show();
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

        private async Task CloseStartify()
        {
            await Engine.ShowByeNotification();
            System.Windows.Application.Current.Shutdown();
        }

        private async void startmenubasewindow_LostFocus(object sender, RoutedEventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            Task animationTask = startPlaceholder.StartCloseStartAnimation();
            await animationTask; // wait for the animation task to finish
            Hide();
        }
    }
}
