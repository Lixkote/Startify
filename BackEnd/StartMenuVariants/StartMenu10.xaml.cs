using Microsoft.Win32;
using ModernWpf;
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

    public partial class StartMenu10 : Window
    {
        Helpers.WinButtonHook StartListener;
        Helpers.RegistryMonitor AccentListener;
        Helpers.RegistryMonitor ThemeListener;
        public bool applistwasloaded = false;

        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<StartMenuEntry> Pinned = new ObservableCollection<StartMenuEntry>();


        private static string GetHookingMethod(string filePath)
        {
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
        private async void StartMenuActivated(object sender, EventArgs e)
        {
            Show();
            // var startPlaceholder = StartMenuIslandhCompact.Child as Shell.Interface.StartMenuCompact.StartMenuCompact;
            // Task animationTask = startPlaceholder.StartOpenStartAnimation();
            // await animationTask; // wait for the animation task to finish
            this.Focus();
        }

        private async void StartMenuDeactivated(object sender, EventArgs e)
        {
            // var startPlaceholder = StartMenuIslandhCompact.Child as Shell.Interface.StartMenuCompact.StartMenuCompact;
            // Task animationTask = startPlaceholder.StartCloseStartAnimation();
            // await animationTask; // wait for the animation task to finish
            Hide();
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
        public StartMenu10()
        {
            // Initialize Startify
            InitializeComponent();
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

                        case "Standard":
                            StartListener = new WinButtonHook();
                            StartListener.StartTriggered += OnStartTriggered;
                            StartListener.FindAndActivateWindow();
                            break;

                        default:
                            ModernWpf.MessageBox.Show("Invalid Start Button hooking method specified. Default(Standard) type will be used.", "Startify Misconfiguration Detected", MessageBoxButton.OK, SymbolGlyph.Warning, MessageBoxResult.OK);
                            StartListener = new WinButtonHook();
                            StartListener.StartTriggered += OnStartTriggered;
                            StartListener.FindAndActivateWindow();
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
            async void OnStartTriggered(object sender, EventArgs e)
            {
                var newVisibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

                if (newVisibility == Visibility.Hidden)
                {
                    var startPlaceholder = StartMenuIslandhCompact.Child as Shell.Interface.StartMenuCompact.StartMenuCompact;
                    // Task animationTask = startPlaceholder.StartCloseStartAnimation();
                    // await animationTask; // Wait for the animation task to finish
                    Hide(); // Hide the window
                }

                Visibility = newVisibility;

                if (Visibility == Visibility.Visible)
                {
                    Show();
                    WindowActivator.ActivateWindow(new System.Windows.Interop.WindowInteropHelper(StartMenu10Host).Handle);
                    this.Focus();
                }
            }
            // Do this so the app wont wait for user start button press on startup.
            Show();
            Hide();
            AlignStartifyWithTaskbar(this);
        }
    }
}
