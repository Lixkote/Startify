using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace Shell.Shell.ShellUtils
{
    public class TileTempleteSelector : DataTemplateSelector
    {
        public DataTemplate Tile1 { get; set; }
        public DataTemplate Tile2 { get; set; }
        public DataTemplate Tile3 { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item.ToString() == "WPF.Helpers.Tile1")
            {
                return Tile1;
            }
            else if (item.ToString() == "WPF.Helpers.Tile2")
            {
                return Tile2;
            }
            else if (item.ToString() == "WPF.Helpers.Tile3")
            {
                return Tile3;
            }
            else
            {
                return Tile2;
            }
        }
    }
}
