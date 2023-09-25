using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace WPF.Helpers.Launching
{
    internal class Shutdown
    {
        public void Deinitialize(ExitEventArgs e)
        {
            UnhookWindowsStartMenu(e);
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public const int restorewin = 1;

        public void UnhookWindowsStartMenu(ExitEventArgs e)
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
