using IWshRuntimeLibrary;
using Microsoft.Toolkit.Uwp.Notifications;
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WPF.Views
{
    /// <summary>
    /// Interaction logic for StartMenu.xaml
    /// </summary>
    public partial class StartMenu : Window
    {
        Helpers.StartMenuListener StartListener;
        Helpers.ProgramsApps.LaunchAppProgram AppLauncher = new Helpers.ProgramsApps.LaunchAppProgram();
        Helpers.ProgramsApps.ProgramGetHelper AppHelper = new Helpers.ProgramsApps.ProgramGetHelper();
        Helpers.Launching.Startup startup = new Helpers.Launching.Startup();
        Helpers.StartMenuTools startMenuTools = new Helpers.StartMenuTools();

        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        public StartMenu()
        {
            // Initialize Startify
            InitializeComponent();
            StartListener = new StartMenuListener();
            StartListener.StartTriggered += OnStartTriggered;
            StartListener.ListenerErrorHappened += StartifyErrorOccured;
            StartListener.FindAndActivateWindow();
            startMenuTools.AlignStartifyWithTaskbar(this);
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            // Do this so the app wont wait for user start button press on startup.
            Show();
            Hide();
        }

        private void StartifyErrorOccured(object sender, EventArgs e)
        {
            ShowErrorNotification();
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

        void OnStartTriggered(object sender, EventArgs e)
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            if (Visibility == Visibility.Visible)
            {
                Show();
                WindowActivator.ActivateWindow(new System.Windows.Interop.WindowInteropHelper(startmenubasewindow).Handle);
                this.Focus();
            }
        }
        private void StartMenuActivated(object sender, EventArgs e)
        {
            startMenuTools.AlignStartifyWithTaskbar(this);
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
            var cvs = startPlaceholder.FindName("cvs") as Windows.UI.Xaml.Data.CollectionViewSource;

            allAppsListView.ItemClick += (sender, e) => AppLauncher.AppLaunchHandler(sender, e, this, Programs, false);

            cvs.Source = from p in Programs
                         orderby p.Alph
                         group p by char.IsDigit(p.Alph[0]) ? "#" : p.Alph into g
                         select g;
            allAppsListView.Loaded += applistloaded;

            var hibernate = startPlaceholder.FindName("HibernateMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var sleep = startPlaceholder.FindName("SleepMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var restart = startPlaceholder.FindName("RestartMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;
            var power = startPlaceholder.FindName("PowerMenuButton") as Windows.UI.Xaml.Controls.MenuFlyoutItem;

            var powerKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Power");
            int hibernateValue = (int)powerKey.GetValue("HibernateEnabledDefault");
            hibernate.Visibility = (hibernateValue == 1) ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;

            hibernate.Click += async (sender, e) => await HibernateAsync();
            sleep.Click += async (sender, e) => await SleepAsync();
            power.Click += async (sender, e) => await ShutdownAsync();
            restart.Click += async (sender, e) => await RestartAsync();

            // startPlaceholder.OpenFileLocationClicked += async (sender, e) => await OpenUninstall();
            // startPlaceholder.RunAsAdminClicked += async (sender, e) => await AppLauncher.DirectoryAppLaunchHandler(sender, e, this, Programs, false);
            startPlaceholder.UninstallSettingsShouldOpen += async (sender, e) => await OpenUninstall();

            var colorization = startPlaceholder.FindName("IsColorizationEnabled") as Windows.UI.Xaml.Controls.TextBlock;
            int themevalue = (int)Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize").GetValue("ColorPrevalence");
            colorization.Text = themevalue.ToString();
            ShowNotification();
        }
        private void applistloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;
            // Find the ListView element by its name
            startPlaceholder.DirectoryChildClicked += (sender, e) => AppLauncher.DirectoryAppLaunchHandler(sender, e, this, Programs, false);
        }
        private void startmenubasewindow_LostFocus(object sender, RoutedEventArgs e)
        {
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
