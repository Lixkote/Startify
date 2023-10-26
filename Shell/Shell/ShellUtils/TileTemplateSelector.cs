using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Diagnostics;
using static Shell.Shell.ShellUtils.TileTempleteSelector;

namespace Shell.Shell.ShellUtils
{
    public class TileTempleteSelector : DataTemplateSelector
    {
        public DataTemplate Tile1 { get; set; }
        public DataTemplate Tile2 { get; set; }
        public DataTemplate Tile3 { get; set; }
        public class Tile : IHasSize
        {
            public string DisplayName { get; set; }
            public string Path { get; set; }
            public string PathUWP { get; set; }
            public string Size { get; set; }
            public Windows.UI.Xaml.Media.Imaging.BitmapImage Icon { get; set; }
            public string IsLiveTileEnabled { get; set; }
            public string TileCustomColor { get; set; }
        }

        public interface IHasSize
        {
            string Size { get; }
        }


        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IHasSize itemWithSize)
            {
                Debug.WriteLine("Our debugitem is: " + itemWithSize.Size);


                if (itemWithSize.Size == "Small")
                {
                    return Tile1;
                }
                else if (itemWithSize.Size == "Normal")
                {
                    return Tile2;
                }
                else if (itemWithSize.Size == "Wide")
                {
                    return Tile3;
                }
            }

            // If the item is not of type Tile or doesn't implement IHasSize, return a default template.
            return Tile2;
        }

    }
}
