using ModernWpf;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using WPF.Helpers;
using System.IO;
using File = System.IO.File;
using MessageBox = ModernWpf.MessageBox;
using StartifyBackend.Helpers;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF.Views;

namespace WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Events
        public event EventHandler StartTrigger;
        public event EventHandler<EventArgs> FailedToLoadTiles;
        public event EventHandler ApplyNewAccent;
        public event EventHandler ApplyNewShellTheme;


        Helpers.WinButtonHook StartListener;
        Helpers.RegistryMonitor AccentListener;
        Helpers.RegistryMonitor ThemeListener;
        AppLauncher AppLauncher = new AppLauncher();
        ProgramLoader AppHelper = new ProgramLoader();
        TilesManager TileAppHelper = new Helpers.TilesManager();
        public bool applistwasloaded = false;
        public const int restorewin = 1;
        const uint EWX_LOGOFF = 0x00000000;


        // Win32 API Imports
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        static extern byte MapVirtualKey(byte wCode, int wMap);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<StartMenuEntry> PinnedForCompactMenu = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<TileGroup> TileGroups = new ObservableCollection<TileGroup>();

        private static string GetHookingMethod(string filePath)
        {
            /////////////////////////////////////////////
            /// Config file helper.
            /////////////////////////////////////////////
            foreach (string line in System.IO.File.ReadLines(filePath))
            {
                string[] keyValue = line.Split('=');
                if (keyValue.Length == 2 && keyValue[0].Trim() == "HookingMethod")
                {
                    return keyValue[1].Trim();
                }
            }

            return string.Empty;
        }

        public static bool IsHibernateEnabled()
        {
            /////////////////////////////////////////////
            /// Determines if the hibernate button should be shown.
            /////////////////////////////////////////////
            
            // Open the registry key for power settings
            RegistryKey powerKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Power");

            // Get the value of HibernateEnabled
            int hibernateValue = (int)powerKey.GetValue("HibernateEnabled");

            // Return true if HibernateEnabled is 1, false otherwise
            return hibernateValue == 1;
        }


        public async Task HibernateAsync()
        {
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Hibernate, true, true);
        }

        public async Task SleepAsync()
        {
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, true, true);
        }
        public async Task ShutdownAsync()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
        }
        public async Task RestartAsync()
        {
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }
        public async Task AccountSettingsAsync()
        {
            Process.Start(new ProcessStartInfo("ms-settings:yourinfo") { UseShellExecute = true });
        }
        public async Task SignoutAsync()
        {
            // Exit the WPF application
            System.Windows.Application.Current.Shutdown();
            // Log off the current user
            ExitWindowsEx(EWX_LOGOFF, 0);
        }
        public async Task LockAsync()
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




        public void Application_Startup(object sender, StartupEventArgs e)
        {
            /////////////////////////////////////////////
            /// Here we load the main configuration file and set the hooking method from it.
            /////////////////////////////////////////////
            string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Startify", "settings.cfg");

            try
            {
                if (File.Exists(configFilePath))
                {
                    string hookingMethod = GetHookingMethod(configFilePath);
                    switch (hookingMethod.ToUpperInvariant())
                    {
                        case "AHK":
                            ModernWpf.MessageBox.Show("AutoHotKey Start Button Hooking Method Selected.                                 Note: This mode is prototype and might misbehave or not work at all. Check Startify Documentation before using this.", "Startify Prototype Features", MessageBoxButton.OK, SymbolGlyph.Construction, MessageBoxResult.OK);
                            break;

                        case "STANDARD":
                            StartListener = new WinButtonHook();
                            StartListener.StartTriggered += OnStartTriggered;
                            StartListener.FindAndActivateWindow();
                            break;

                        default:
                            ModernWpf.MessageBox.Show("Invalid Start Button hooking method specified. Default(Standard) type will be used.", "Startify Misconfiguration Detected", MessageBoxButton.OK, SymbolGlyph.Warning, MessageBoxResult.OK);

                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Configuration file not found: " + configFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading configuration file: " + ex.Message);
            }
            StartListener = new WinButtonHook();
            StartListener.StartTriggered += OnStartTriggered;
            StartListener.FindAndActivateWindow();
            /////////////////////////////////////////////
            /// Here we load the listeners for various things, like themes or other stuff.
            /////////////////////////////////////////////
            AccentListener = new RegistryMonitor(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            AccentListener.RegChanged += ApplyNewAccent;
            AccentListener.Start();

            ThemeListener = new RegistryMonitor(RegistryHive.CurrentUser, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
            ThemeListener.RegChanged += ApplyNewShellTheme;
            ThemeListener.Start();

            TileAppHelper.CouldNotLoadTiles += FailedToLoadTiles;

            // Create the startup window
            StartMenu StartMena = new StartMenu();
            StartMena.Show();
        }

        public void OnStartTriggered(object sender, EventArgs e)
        {
            StartTrigger?.Invoke(sender, e);
        }


        public async void ShowStolenTiles()
        {
            /////////////////////////////////////////////
            /// Failsafe message for when there was a trouble loading tiles.
            /////////////////////////////////////////////

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
                .AddText("Tiles disabled.")
                .AddText("We could not find or load the tiles layout file.")
                .Show();
        }


        public async void ShowNotification()
        {
            /////////////////////////////////////////////
            /// Welcome message
            /////////////////////////////////////////////
           
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
                .AddText("Welcome to Startify(Preview)!")
                .AddText("Check it out by clicking the Windows Start button!")
                .Show();
        }

        public void ShowErrorNotification(string reason)
        {
            /////////////////////////////////////////////
            /// Error message.
            /////////////////////////////////////////////
            
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
                .AddText(":C")
                .AddText("Startify ran into an issue. Error reason:" + reason)
                .Show();
        }

        public async Task ShowByeNotification()
        {
            /////////////////////////////////////////////
            /// "Goodbye" message.
            /////////////////////////////////////////////
            
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

        protected override void OnExit(ExitEventArgs e)
        {            
            /////////////////////////////////////////////
            /// Here we unhook and fix windows 11 start menu.
            /////////////////////////////////////////////
            
            // "Unhide" the stock windows 11 start menu so that it works again.
            string caption = "Start";
            string className = "Windows.UI.Core.CoreWindow";

            IntPtr windowHandle = FindWindow(className, caption);

            if (windowHandle != IntPtr.Zero)
            {
                ShowWindow(windowHandle, restorewin);
                SetForegroundWindow(windowHandle);
            }
            else
            {
                Debug.WriteLine("Startify has issues unhooking the Windows 11's Start Menu. It may not work correctly until the next system restart.");
            }
        }
    }
}
