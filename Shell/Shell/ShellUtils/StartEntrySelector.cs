using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;

namespace Shell.Shell.ShellUtils
{
    public class StartMenuSelector : DataTemplateSelector
    {
        public DataTemplate AppD { get; set; }
        public DataTemplate Folder { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item.ToString() == "WPF.Helpers.StartMenuLink")
            {
                return AppD;
            }
            else if (item.ToString() == "WPF.Helpers.StartMenuDirectory")
            {
                return Folder;
            }
            else
            {
                return base.SelectTemplateCore(item);
            }
        }
    }
}
