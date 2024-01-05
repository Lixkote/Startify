using ManagedShell;
using ManagedShell.AppBar;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using WPF.Views;

namespace WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public const int restorewin = 1;
        protected override void OnExit(ExitEventArgs e)
        {
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
