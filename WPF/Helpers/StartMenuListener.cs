using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Threading;

namespace WPF.Helpers
{
    internal class StartMenuListener
    {
        public StartMenuListener()
        {
            this.MouseCallback += new HookProc(MouseEvents);
            this.KeyCallback += new HookProc(KeyEvents);
            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                IntPtr hModule = GetModuleHandle(module.ModuleName);
                _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, this.MouseCallback, hModule, 0);
                _keyHook = SetWindowsHookEx(WH_KEY_LL, this.KeyCallback, hModule, 0);
            }
        }
        int WH_KEY_LL = 13;
        int WH_MOUSE_LL = 14;
        int HC_ACTION = 0;
        HookProc MouseCallback = null;
        HookProc KeyCallback = null;
        IntPtr _mouseHook = IntPtr.Zero;
        IntPtr _keyHook = IntPtr.Zero;

        // Define the ShowWindowCommand enum
        enum ShowWindowCommand
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
            // Other values omitted for brevity
        }
        public const int restorewin = 1;
        public const int closewin = 0;

        public event EventHandler<EventArgs> StartTriggered;

        public static IntPtr FindWindowByCaptionAndClass(string caption, string className)
        {
            IntPtr shellTrayWnd = FindWindowExA(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);

            if (shellTrayWnd != IntPtr.Zero)
            {
                IntPtr startButton = FindWindowExA(shellTrayWnd, IntPtr.Zero, className, caption);

                if (startButton != IntPtr.Zero)
                {
                    // You've found the window; you can now interact with it using its handle.
                    return startButton;
                }
            }

            return IntPtr.Zero; // Window not found
        }

        public void FindAndActivateWindow()
        {
            string caption = "Start";
            string className = "Start";

            IntPtr windowHandle = FindWindowByCaptionAndClass(caption, className);

            if (windowHandle != IntPtr.Zero)
            {
                // Unhide the window (restore it)
                ShowWindow(windowHandle, restorewin);

                // Bring the window to the foreground
                SetForegroundWindow(windowHandle);
            }
            else
            {
                MessageBox.Show("Error hooking Windows 11's Start Button :C");
            }
        }

        public void FindAndCloseW11StartWindow()
        {
            string caption = "Start";
            string className = "Windows.UI.Core.CoreWindow";

            IntPtr windowHandle = FindWindow(className, caption);

            if (windowHandle != IntPtr.Zero)
            {
                // Unhide the window (restore it)
                ShowWindow(windowHandle, closewin);

                // Bring the window to the foreground
                SetForegroundWindow(windowHandle);
            }
            else
            {
                MessageBox.Show("Error closing Windows 11's Start Menu :C");
            }
        }


        public int MouseEvents(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(_mouseHook, code, wParam, lParam);

            if (code == HC_ACTION)
            {
                if (wParam.ToInt32() == (uint)WM.WM_LBUTTONDOWN)
                {
                    MSLLHOOKSTRUCT ms = new MSLLHOOKSTRUCT();
                    ms = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    IntPtr win = WindowFromPoint(ms.pt);
                    string title = GetWindowTextRaw(win);

                    StringBuilder className = new StringBuilder(256);
                    GetClassName(win, className, className.Capacity);
                    string win32ClassName = className.ToString();

                    if (win32ClassName == "Start")
                    {
                        // Trigger your event
                        StartTriggered(this, null);
                        return 1;
                    }
                }
            }
            return CallNextHookEx(_mouseHook, code, wParam, lParam);
        }
        Stopwatch stopwatch = new Stopwatch();
        HashSet<Keys> pressedKeys = new HashSet<Keys>();

        int KeyEvents(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(_mouseHook, code, wParam, lParam);

            if (code == this.HC_ACTION)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                if (wParam == (IntPtr)0x0100) // Key down
                {
                    pressedKeys.Add(objKeyInfo.key); // Add the key to the set of pressed keys
                    stopwatch.Restart(); // Restart the stopwatch
                }
                else if (wParam == (IntPtr)0x0101) // Key up
                {
                    pressedKeys.Remove(objKeyInfo.key); // Remove the key from the set of pressed keys
                    stopwatch.Stop(); // Stop the stopwatch

                    if (pressedKeys.Count == 0 && objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.RWin)
                    {
                        // Only execute the actions if all keys are released, and the released key is the Windows key
                        FindAndCloseW11StartWindow();
                        StartTriggered(this, null);
                    }
                }
            }
            return CallNextHookEx(_mouseHook, code, wParam, lParam);
        }


        public void Close()
        {
            if (_mouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHook);
            }
        }
        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr FindWindowExA(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);


        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [Out] StringBuilder lParam);

        public static string GetWindowTextRaw(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(200);
            SendMessage(hwnd, (int)WM.WM_GETTEXT, (IntPtr)sb.Capacity, sb);
            return sb.ToString();
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public Keys key;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr extra;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public UIntPtr dwExtraInfo;
    }
    enum WM : uint
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_GETTEXT = 0x000D,
        WM_GETTEXTLENGTH = 0x000E
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
