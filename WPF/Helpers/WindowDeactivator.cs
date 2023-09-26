using System;
using System.Runtime.InteropServices;

namespace WPF.Helpers
{
    internal class WindowDeactivator
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const int ALT = 0xA4;
        private const int EXTENDEDKEY = 0x1;
        private const int KEYUP = 0x2;
        private const int SW_MINIMIZE = 6;

        public static void DeactivateWindow(IntPtr mainWindowHandle)
        {
            // Check if already has focus
            if (mainWindowHandle != GetForegroundWindow())
            {
                // Simulate an Alt key press
                keybd_event((byte)ALT, 0x45, EXTENDEDKEY | 0, 0);

                // Activate the window
                SetForegroundWindow(mainWindowHandle);

                // Simulate an Alt key release
                keybd_event((byte)ALT, 0x45, EXTENDEDKEY | KEYUP, 0);
            }

            // Minimize the window
            ShowWindow(mainWindowHandle, SW_MINIMIZE);
        }
    }
}
