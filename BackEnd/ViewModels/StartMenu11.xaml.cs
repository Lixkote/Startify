using IWshRuntimeLibrary;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Wpf.UI.XamlHost;
using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using Shell.Interface.StartMenu;
using ShellApp;
using Shell.Interface.StartMenu11;
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
using CommunityToolkit.WinUI;

namespace WPF.Views
{
    /// <summary>
    /// Interaction logic for StartMenu.xaml
    /// </summary>
    public partial class StartMenu11 : System.Windows.Window
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
        string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Startify", "Settings.cfg");

        ////////////////////////////////////////////////////////////////////////////
        // Custom Items under this comment, specific for this start menu type
        // So like, this start menu uses tiles, so here will be the tile specific stuff
        ////////////////////////////////////////////////////////////////////////////

        private void DisableTiles(object sender, EventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            var StartBackground = startPlaceholder.FindName("startbackground") as Windows.UI.Xaml.Controls.Grid;
            StartBackground.Width = 310;
            Engine.ShowStolenTiles();
        }
        private void DisableTilesSilent()
        {
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            var StartBackground = startPlaceholder.FindName("startbackground") as Windows.UI.Xaml.Controls.Grid;
            StartBackground.Width = 310;
        }

        public async void LoadTiles()
        {
            TileGroups = new ObservableCollection<TileGroup>();
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            await TileAppHelper.LoadTileGroups(TileGroups);

            var tilegridview = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            tilegridview.ItemsSource = TileGroups;
        }

        public async void ReloadTiles()
        {
            TileGroups = new ObservableCollection<TileGroup>();
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            await TileAppHelper.LoadTileGroups(TileGroups);

            var tilegridview = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            tilegridview.ItemsSource = null;
            tilegridview.ItemsSource = TileGroups;
        }

        private void TilesLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            var allAppsListViewBase = startPlaceholder.FindName("AllApps") as Shell.Interface.StartMenu11.Controls.AllAppsPaneControl;
            // Find the ListView element by its name
            startPlaceholder.TileClickedMain += (sender, e) => TileAppHelper.OpenTileApp(sender, e, this, false);
            startPlaceholder.TileUnpinnedMain += (sender, e) => UnpinTile(sender, e);
            allAppsListViewBase.TilePinnedMain += (sender, e) => PinTile(sender, e);
            startPlaceholder.TileGroupRenamedMain += (sender, e) => ChangeGroupName(sender, e);
            // startPlaceholder.TileSizeChangedMain += (sender, e) => ChangeTileSize(sender, e);
        }

        public void ChangeTileSize(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Debug.WriteLine("Tile size change method called.");
            TileAppHelper.ChangeTileSizeInXML(sender, TileGroups);
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            var tileGroupGridView = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            LoadTiles();
            tileGroupGridView.ItemsSource = null;
            tileGroupGridView.ItemsSource = TileGroups;
        }

        public void UnpinTile(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Debug statement to check if the method is being called
            Debug.WriteLine("UnpinTile method called.");
            TileAppHelper.RemoveTileFromXml(sender, TileGroups);
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            var tileGroupGridView = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            LoadTiles();
            tileGroupGridView.ItemsSource = null;
            tileGroupGridView.ItemsSource = TileGroups;
        }

        public void ChangeGroupName(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TileAppHelper.ChangeGroupName(sender, TileGroups);
        }

        public void RefreshAppList()
        {
            if (applistwasloaded == true)
            {
                var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
                var allAppsListViewBase = startPlaceholder.FindName("AllApps") as Windows.UI.Xaml.Controls.UserControl;
                var allAppsListView = allAppsListViewBase.FindName("AllAppsListView") as Windows.UI.Xaml.Controls.ListView;
                if (allAppsListView.Items.Count > 0)
                {
                    var firstItem = allAppsListView.Items[0]; // Get the first item in your data source
                    allAppsListView.ScrollIntoView(firstItem);
                }
                var cvs = allAppsListViewBase.FindName("cvs") as Windows.UI.Xaml.Data.CollectionViewSource;
                cvs.Source = from p in Programs
                             orderby p.Alph
                             group p by AppAlphHelper.GetAppAlph(p.Alph) into g
                             select g;
            }
        }

        public void PinTile(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Debug statement to check if the method is being called
            Debug.WriteLine("PinTile method called.");
            TileAppHelper.AddTileToXml(sender, TileGroups);
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            var tileGroupGridView = startPlaceholder.FindName("TileGroupGridView") as Windows.UI.Xaml.Controls.GridView;
            LoadTiles();
            tileGroupGridView.ItemsSource = null;
            tileGroupGridView.ItemsSource = TileGroups;
        }

        private void applistloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            // Find the ListView element by its name
            startPlaceholder.DirectoryChildClicked += (sender, e) => Launcher.DirectoryAppLaunchHandler(sender, e, this, Programs, false);
            applistwasloaded = true;
        }

        private static string GetConfigFileEntry(string filePath, string entry)
        {
            /////////////////////////////////////////////
            /// Config file helper.
            /////////////////////////////////////////////
            foreach (string line in System.IO.File.ReadLines(filePath))
            {
                string[] keyValue = line.Split('=');
                if (keyValue.Length == 2 && keyValue[0].Trim() == entry)
                {
                    return keyValue[1].Trim();
                }
            }

            return string.Empty;
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

            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            var allAppsListViewBase = startPlaceholder.FindName("AllApps") as Windows.UI.Xaml.Controls.UserControl;
            var allAppsListView = allAppsListViewBase.FindName("AllAppsListView") as Windows.UI.Xaml.Controls.ListView;
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
            if (!File.Exists(configFilePath))
            {
                try
                {
                    // Ensure the directory exists
                    string directoryPath = Path.GetDirectoryName(configFilePath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Template content
                    string templateContent = "DockedDesign=false\n" +
                                             "DisplayTiles=true\n" +
                                             "ShowSettingsButton=true\n" +
                                             "ShowExplorerButton=true\n" +
                                             "ShowDocumentsButton=true\n" +
                                             "ShowDownloadsButton=true\n" +
                                             "ShowMusicButton=false\n" +
                                             "ShowPicturesButton=false\n" +
                                             "ShowMoviesButton=false\n" +
                                             "ShowNetworkButton=false\n" +
                                             "ShowPersonalFolderButton=false\n" +
                                             "TooltipCaption=Start\n" +
                                             "TooltipName=Start";

                    // Write the template content to the file
                    File.WriteAllText(configFilePath, templateContent);

                    // Update the status
                    ModernWpf.MessageBox.Show("Config file created successfully. Press OK to continue.", "Startify first run completed", MessageBoxButton.OK, SymbolGlyph.Info, MessageBoxResult.OK);
                    ApplySettings(startPlaceholder);
                }
                catch (Exception ex)
                {
                    ModernWpf.MessageBox.Show("More information: \n" + ex.ToString(), "Startify first run failed", MessageBoxButton.OK, SymbolGlyph.Info, MessageBoxResult.OK);
                }
            }
            else
            {
                // Config file already exists
                ApplySettings(startPlaceholder);
            }
        }



        private static void SetButtonVisibility(string configFilePath, string settingName, string buttonName, Shell.Interface.StartMenu11.StartMenu startPlaceholder)
        {
            string settingValue = GetConfigFileEntry(configFilePath, settingName);
            var button = startPlaceholder.FindName(buttonName) as Windows.UI.Xaml.Controls.Button;

            if (button != null)
            {
                if (settingValue == "true")
                {
                    button.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else 
                {
                    button.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }


        public void ApplySettings(Shell.Interface.StartMenu11.StartMenu startPlaceholder)
        {

            // Handle DockedDesign separately as it has a unique action
            string dockedDesignSetting = GetConfigFileEntry(configFilePath, "DockedDesign");
            if (dockedDesignSetting.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                startPlaceholder.EnableDockedDesign();
            }

            // Handle DisplayTiles separately as it has a unique action
            string displayTilesSetting = GetConfigFileEntry(configFilePath, "DisplayTiles");
            if (displayTilesSetting.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                DisableTilesSilent();
            }

            // Handle visibility settings for buttons
            SetButtonVisibility(configFilePath, "ShowSettingsButton", "SettingsAppButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowExplorerButton", "FileExplorerButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowDocumentsButton", "DocumentsButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowDownloadsButton", "DownloadsButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowMusicButton", "MusicButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowPicturesButton", "PicturesButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowMoviesButton", "MoviesButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowNetworkButton", "NetworkButton", startPlaceholder);
            SetButtonVisibility(configFilePath, "ShowPersonalFolderButton", "PersonalFolderButton", startPlaceholder);
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

        public StartMenu11()
        {
            // Initialize Normal Tiled Start Menu Layout
            InitializeComponent();

            // Connect to the StartifyEngine
            Engine.FailedToLoadTiles += DisableTiles;
            Engine.ApplyNewAccent += ApplyNewAccent;
            Engine.ApplyNewShellTheme += ApplyNewShellTheme;
            Engine.StartTrigger += ToggleStartMenu;

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

                if (alignkey != null)
                {
                    object alignValueObj = alignkey.GetValue("TaskbarAl");
                    if (alignValueObj != null && int.TryParse(alignValueObj.ToString(), out int alignvalue))
                    {
                        if (alignvalue == 0)
                        {
                            var desktopWorkingArea = SystemParameters.WorkArea;
                            window.Left = 0;
                            window.Top = desktopWorkingArea.Bottom - window.Height;
                        }
                        else if (alignvalue == 1)
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
                    else
                    {
                        var desktopWorkingArea = SystemParameters.WorkArea;
                        window.Left = 0;
                        window.Top = desktopWorkingArea.Bottom - window.Height;
                    }
                }
                else
                {
                    // Handle the case where the registry key doesn't exist
                    ModernWpf.MessageBox.Show("Startify has issues reading the taskbar alignment registry key. The default (left) alignment will be used.", "Startify Backend Error", MessageBoxButton.OK, SymbolGlyph.Error, MessageBoxResult.OK);
                }
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                ModernWpf.MessageBox.Show("An error occurred while aligning Startify with the taskbar: " + ex.Message, "Startify Backend Error", MessageBoxButton.OK, SymbolGlyph.Error, MessageBoxResult.OK);
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
                    var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;

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
                var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
                startPlaceholder.AccentWasEnabled();
            }
            else if (themeValue == 0)
            {
                var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
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
                    var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;

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

        public async void ToggleStartMenu(object sender, EventArgs e)
        {
            try
            {
                if (Visibility == Visibility.Visible)
                {
                    HideStartMenu(sender, e);
                }
                else
                {
                    ShowStartMenu(sender, e);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public async void ShowStartMenu(object sender, EventArgs e)
        {
            RefreshAppList();
            Show();
            WindowActivator.ActivateWindow(new System.Windows.Interop.WindowInteropHelper(StartMenu11Host).Handle); // Activate window
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            await startPlaceholder.StartOpenStartAnimation(); // Wait for the animation
            this.Focus();
        }

        public async void HideStartMenu(object sender, EventArgs e)
        {
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            await startPlaceholder.StartCloseStartAnimation(); // Wait for the animation
            Hide();
        }

        public async void HideStartMenuNoEvent()
        {
            var startPlaceholder = StartMenuIslandh.Child as Shell.Interface.StartMenu11.StartMenu;
            await startPlaceholder.StartCloseStartAnimation(); // Wait for the animation
            Hide();
        }

        private void HandleError(Exception ex)
        {
            // Handle any exceptions
            Console.WriteLine($"Error: {ex.Message}");
        }



        private async Task CloseStartify()
        {
            await Engine.ShowByeNotification();
            System.Windows.Application.Current.Shutdown();
        }
    }
}
