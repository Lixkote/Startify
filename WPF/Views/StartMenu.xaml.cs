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

        ObservableCollection<StartMenuEntry> Programs = new ObservableCollection<StartMenuEntry>();
        ObservableCollection<StartMenuLink> Results = new ObservableCollection<StartMenuLink>();

        private void StartMenuIsland_Loaded(object sender, RoutedEventArgs e)
        {
            // this hides hibernation button if it should be hiddeen
            // put the hibernation hidding code here... //
            // this is responsible for filling the xaml listview with programs and shit
            string programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            GetPrograms(programs);
            programs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            GetPrograms(programs);
            Programs = new ObservableCollection<StartMenuEntry>(Programs.OrderBy(x => x.Title));

            // Get the StartPlaceholder object from the WindowsXamlHost element
            var startPlaceholder = StartMenuIslandh.Child as ShellApp.Shell.Start.StartPlaceholder;

            // Find the ListView element by its name
            var allAppsListView = startPlaceholder.FindName("AllAppsListView") as Windows.UI.Xaml.Controls.ListView;

            // Set the ItemsSource property of the ListView element to Programs
            allAppsListView.ItemsSource = Programs;





        }


        private void GetPrograms(string directory)
        {
            foreach (string f in Directory.GetFiles(directory))
            {
                if (System.IO.Path.GetExtension(f) != ".ini")
                {
                    Programs.Add(new StartMenuLink
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(f),
                        Icon = IconHelper.GetFileIcon(f),
                        Link = f
                    });
                }
            }
            GetProgramsRecurse(directory);
        }
        private void GetProgramsRecurse(string directory, StartMenuDirectory parent = null)
        {
            bool hasParent = parent != null;
            foreach (string d in Directory.GetDirectories(directory))
            {
                StartMenuDirectory folderEntry = null;
                if (!hasParent)
                {
                    folderEntry = Programs.FirstOrDefault(x => x.Title == new DirectoryInfo(d).Name) as StartMenuDirectory;
                }
                if (folderEntry == null)
                {
                    folderEntry = new StartMenuDirectory
                    {
                        Title = new DirectoryInfo(d).Name,
                        Links = new ObservableCollection<StartMenuLink>(),
                        Directories = new ObservableCollection<StartMenuDirectory>(),
                        Link = d,
                        Icon = IconHelper.GetFolderIcon(d)
                    };
                }

                GetProgramsRecurse(d, folderEntry);
                foreach (string f in Directory.GetFiles(d))
                {
                    folderEntry.HasChildren = true;
                    if (System.IO.Path.GetExtension(f) != ".ini")
                    {
                        folderEntry.Links.Add(new StartMenuLink
                        {
                            Title = System.IO.Path.GetFileNameWithoutExtension(f),
                            Icon = IconHelper.GetFileIcon(f),
                            Link = f
                        });
                    }
                }

                if (!hasParent)
                {
                    if (!Programs.Contains(folderEntry))
                    {
                        Programs.Add(folderEntry);
                    }
                }
                else
                {
                    parent.Directories.Add(folderEntry);
                }
            }
        }



    }
}
