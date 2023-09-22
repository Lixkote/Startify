using ManagedShell;
using ManagedShell.AppBar;
using System;
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
            //Restore stock start menu to work again
            string caption = "Start";
            string className = "Windows.UI.Core.CoreWindow";

            IntPtr windowHandle = FindWindow(className, caption);

            if (windowHandle != IntPtr.Zero)
            {
                // Unhide the window (restore it)
                ShowWindow(windowHandle, restorewin);

                // Bring the window to the foreground
                SetForegroundWindow(windowHandle);
            }
            else
            {
                MessageBox.Show("Error closing Windows 11's Start Menu :C");
            }
        }
    }
}
