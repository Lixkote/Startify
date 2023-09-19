using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

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

        private enum SW
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
        }


        public event EventHandler<EventArgs> StartTriggered;

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
                        // Show the hidden "Start" window
                        ShowWindow(win, (int)SW.SW_SHOWNORMAL);

                        // Trigger your event
                        StartTriggered(this, null);
                        return 1;
                    }
                }
            }
            return CallNextHookEx(_mouseHook, code, wParam, lParam);
        }

        int KeyEvents(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(_mouseHook, code, wParam, lParam);

            if (code == this.HC_ACTION)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                if (wParam == (IntPtr)256 && (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin))
                {
                    StartTriggered(this, null);
                    return 1;
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
