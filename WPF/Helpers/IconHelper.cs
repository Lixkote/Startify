using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using WPF.Helpers;

namespace WPF.Helpers
{
    internal static class IconHelper
    {

        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        //Constants flags for SHGetFileInfo 
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SYSICONINDEX = 0x000004000;
        public const uint ILD_NORMAL = 0x0000;

        //Import SHGetFileInfo function
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        public static ImageSource GetFolderIcon(string dir)
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            SHGetFileInfo(
                dir,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);

            using (Icon icn = System.Drawing.Icon.FromHandle(shinfo.hIcon))
                return icn.ToBitmap().ToBitmapImage();
        }
        static Dictionary<string, ImageSource> _iconCache = new Dictionary<string, ImageSource>();
        static Dictionary<string, System.Uri> _iconCachea = new Dictionary<string, System.Uri>();
        static string[] _exlcudedIcons = new string[] { ".exe", ".lnk", ".url", ".appref-ms" };
        [DllImport("Comctl32.dll")]
        public static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, uint flags);
        public static string GetFileIcon(string file)
        {
            try
            {
                string extension = System.IO.Path.GetExtension(file);
                {
                    // Check if the .lnk file exists
                    if (!File.Exists(file))
                    {
                        throw new FileNotFoundException("The .lnk file does not exist.");
                    }

                    // Get the target file path from the .lnk file using IWshRuntimeLibrary
                    // Reference: [1](https://stackoverflow.com/questions/1127647/convert-system-drawing-icon-to-system-media-imagesource)
                    IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                    IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(file);
                    string targetFilePath = shortcut.TargetPath;

                    // Check if the target file exists
                    if (!File.Exists(targetFilePath))
                    {
                        throw new FileNotFoundException("The target file does not exist.");
                    }

                    // Extract the icon from the target file using System.Drawing.Icon
                    // Reference: [2](https://stackoverflow.com/questions/3204883/wpf-imagesource-binding-with-custom-converter)
                    Icon icon = Icon.ExtractAssociatedIcon(targetFilePath);

                    // Convert the icon to a BitmapSource using System.Windows.Interop.Imaging
                    // Reference: [3](https://stackoverflow.com/questions/2969821/display-icon-in-wpf-image)
                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    // Create a PngBitmapEncoder and add the BitmapSource to its frames
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    // Fix for icons 22.09.2023
                    string folderPath = @"C:\ProgramData\Startify\IconTemp";

                    if (!Directory.Exists(folderPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(folderPath);
                            Console.WriteLine("Folder created successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating folder: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Folder already exists.");
                    }


                    // Generate a unique file name for the icon file based on the .lnk file name
                    string fileName = Path.GetFileNameWithoutExtension(file) + ".png";
                    string filePath = Path.Combine("C:\\ProgramData\\Startify\\IconTemp", fileName);

                    // Save the icon file to the destination folder using a FileStream
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    string escapedFilePath = Uri.EscapeDataString(filePath);

                    // Return the URI of the icon file as a string
                    return "file:///" + escapedFilePath;
                }
            }
            catch
            {
                string none = "ms-appx:///Assets/placeholder.png";
                return none; 
            }
        }

        [DllImport("shell32.dll", EntryPoint = "#261",
               CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void GetUserTilePath(
          string username,
          UInt32 whatever, // 0x80000000
          StringBuilder picpath, int maxLength);

        public static string GetUserTilePath(string username)
        {   // username: use null for current user
            var sb = new StringBuilder(1000);
            GetUserTilePath(username, 0x80000000, sb, sb.Capacity);
            return sb.ToString();
        }

        public static System.Drawing.Image GetUserTile(string username)
        {
            return System.Drawing.Image.FromFile(GetUserTilePath(username));
        }

        public static System.Drawing.Image OvalImage(System.Drawing.Image img)
        {
            Bitmap bmp = new Bitmap(img.Width, img.Height);
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddEllipse(0, 0, img.Width, img.Height);
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.SetClip(gp);
                    gr.DrawImage(img, System.Drawing.Point.Empty);
                }
            }
            return bmp;
        }

        public enum SHSTOCKICONID : uint
        {
            SIID_DOCNOASSOC = 0,
            SIID_DOCASSOC = 1,
            SIID_APPLICATION = 2,
            SIID_FOLDER = 3,
            SIID_FOLDEROPEN = 4,
            SIID_DRIVE525 = 5,
            SIID_DRIVE35 = 6,
            SIID_DRIVEREMOVE = 7,
            SIID_DRIVEFIXED = 8,
            SIID_DRIVENET = 9,
            SIID_DRIVENETDISABLED = 10,
            SIID_DRIVECD = 11,
            SIID_DRIVERAM = 12,
            SIID_WORLD = 13,
            SIID_SERVER = 15,
            SIID_PRINTER = 16,
            SIID_MYNETWORK = 17,
            SIID_FIND = 22,
            SIID_HELP = 23,
            SIID_SHARE = 28,
            SIID_LINK = 29,
            SIID_SLOWFILE = 30,
            SIID_RECYCLER = 31,
            SIID_RECYCLERFULL = 32,
            SIID_MEDIACDAUDIO = 40,
            SIID_LOCK = 47,
            SIID_AUTOLIST = 49,
            SIID_PRINTERNET = 50,
            SIID_SERVERSHARE = 51,
            SIID_PRINTERFAX = 52,
            SIID_PRINTERFAXNET = 53,
            SIID_PRINTERFILE = 54,
            SIID_STACK = 55,
            SIID_MEDIASVCD = 56,
            SIID_STUFFEDFOLDER = 57,
            SIID_DRIVEUNKNOWN = 58,
            SIID_DRIVEDVD = 59,
            SIID_MEDIADVD = 60,
            SIID_MEDIADVDRAM = 61,
            SIID_MEDIADVDRW = 62,
            SIID_MEDIADVDR = 63,
            SIID_MEDIADVDROM = 64,
            SIID_MEDIACDAUDIOPLUS = 65,
            SIID_MEDIACDRW = 66,
            SIID_MEDIACDR = 67,
            SIID_MEDIACDBURN = 68,
            SIID_MEDIABLANKCD = 69,
            SIID_MEDIACDROM = 70,
            SIID_AUDIOFILES = 71,
            SIID_IMAGEFILES = 72,
            SIID_VIDEOFILES = 73,
            SIID_MIXEDFILES = 74,
            SIID_FOLDERBACK = 75,
            SIID_FOLDERFRONT = 76,
            SIID_SHIELD = 77,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80,
            SIID_KEY = 81,
            SIID_SOFTWARE = 82,
            SIID_RENAME = 83,
            SIID_DELETE = 84,
            SIID_MEDIAAUDIODVD = 85,
            SIID_MEDIAMOVIEDVD = 86,
            SIID_MEDIAENHANCEDCD = 87,
            SIID_MEDIAENHANCEDDVD = 88,
            SIID_MEDIAHDDVD = 89,
            SIID_MEDIABLURAY = 90,
            SIID_MEDIAVCD = 91,
            SIID_MEDIADVDPLUSR = 92,
            SIID_MEDIADVDPLUSRW = 93,
            SIID_DESKTOPPC = 94,
            SIID_MOBILEPC = 95,
            SIID_USERS = 96,
            SIID_MEDIASMARTMEDIA = 97,
            SIID_MEDIACOMPACTFLASH = 98,
            SIID_DEVICECELLPHONE = 99,
            SIID_DEVICECAMERA = 100,
            SIID_DEVICEVIDEOCAMERA = 101,
            SIID_DEVICEAUDIOPLAYER = 102,
            SIID_NETWORKCONNECT = 103,
            SIID_INTERNET = 104,
            SIID_ZIPFILE = 105,
            SIID_SETTINGS = 106,
            SIID_DRIVEHDDVD = 132,
            SIID_DRIVEBD = 133,
            SIID_MEDIAHDDVDROM = 134,
            SIID_MEDIAHDDVDR = 135,
            SIID_MEDIAHDDVDRAM = 136,
            SIID_MEDIABDROM = 137,
            SIID_MEDIABDR = 138,
            SIID_MEDIABDRE = 139,
            SIID_CLUSTEREDDRIVE = 140,
            SIID_MAX_ICONS = 175
        }
        [Flags]
        public enum SHGSI : uint
        {
            SHGSI_ICONLOCATION = 0,
            SHGSI_ICON = 0x000000100,
            SHGSI_SYSICONINDEX = 0x000004000,
            SHGSI_LINKOVERLAY = 0x000008000,
            SHGSI_SELECTED = 0x000010000,
            SHGSI_LARGEICON = 0x000000000,
            SHGSI_SMALLICON = 0x000000001,
            SHGSI_SHELLICONSIZE = 0x000000004
        }
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public Int32 iSysIconIndex;
            public Int32 iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }
        [DllImport("Shell32.dll", SetLastError = false)]
        public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyIcon(IntPtr hIcon);

        public static ImageSource GetUACShield()
        {
            BitmapSource shieldSource = null;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                SHSTOCKICONINFO sii = new SHSTOCKICONINFO();
                sii.cbSize = (UInt32)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

                Marshal.ThrowExceptionForHR(SHGetStockIconInfo(SHSTOCKICONID.SIID_SHIELD,
                    SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON,
                    ref sii));

                shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    sii.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(sii.hIcon);
            }
            else
            {
                shieldSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    System.Drawing.SystemIcons.Shield.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            return shieldSource;
        }
    }
}
