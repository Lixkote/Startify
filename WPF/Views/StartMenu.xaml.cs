using Microsoft.Toolkit.Wpf.UI.XamlHost;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Windows.UI.Xaml.Media;
using WPF.Helpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WPF.Views
{
    /// <summary>
    /// Interaction logic for StartMenu.xaml
    /// </summary>
    public partial class StartMenu : Window
    {
        Helpers.StartMenuListener _listener;
        public StartMenu()
        {
            InitializeComponent();
            _listener = new StartMenuListener();
            _listener.StartTriggered += OnStartTriggered;
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = 0;
            Top = desktopWorkingArea.Bottom - Height;
        }

        void OnStartTriggered(object sender, EventArgs e)
        {

            Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            if (Visibility == Visibility.Visible)
            {
                Show();
                //Storyboard closestartanim = (Storyboard)FindResource("openstartanim");
                //closestartanim.Begin();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Screen screen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
            this.Left = screen.WorkingArea.Left;
        }

        private void Menu_Deactivated(object sender, EventArgs e)
        {
            // Get the storyboard from the resources
            //Storyboard closestartanim = (Storyboard)FindResource("closestartanim");
            //closestartanim.Begin();
            Visibility = Visibility.Hidden;
            Hide();
        }

    }
}
