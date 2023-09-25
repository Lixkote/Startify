using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Threading;
using WPF.Views;
using Microsoft.Win32;
using System.Windows;

namespace WPF.Helpers
{
    internal class StartMenuTools
    {
        public StartMenuTools()
        {

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
                Debug.WriteLine("Startify has issues reading the taskbar alignment registry key. The default(left) alignment will be used. Error code: " + ex.ToString());
            }
        }
    }
}
