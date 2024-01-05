using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using IWshRuntimeLibrary;
using System;
using System.Xml.Serialization;
using System.Xml;

namespace WPF.Helpers
{
    public static class ExtensionMethods
    {
        public static BitmapImage ToBitmapImage(this Image bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        internal static IEnumerable<StartMenuEntry> Search(this IEnumerable<StartMenuEntry> list, string filter)
        {
            var results = list.Where(x =>
            {
                if (x is StartMenuLink && x.Title.ToUpper().Contains(filter.ToUpper()))
                {
                    return true;
                }
                if (x is StartMenuLink link && (link.Link.EndsWith(".lnk")))
                {
                    if (Path.GetFileName(GetShortcutTarget(link)).ToUpper().Contains(filter.ToUpper()))
                    {
                        return true;
                    }
                }
                return false;
            });
            foreach (StartMenuEntry entry in list)
            {
                if (entry is StartMenuDirectory dir)
                {
                    results = results.Union(dir.Links.Search(filter));
                    results = results.Union(dir.Directories.Search(filter));
                }
            }
            return results;
        }

        private static string GetShortcutTarget(StartMenuLink link)
        {
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(link.Link);
            return shortcut.TargetPath;
        }
    }
}
