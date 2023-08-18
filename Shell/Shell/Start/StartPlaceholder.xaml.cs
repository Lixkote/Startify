using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ShellApp.Shell.Start
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StartPlaceholder : UserControl
    {
        public static Storyboard openstartanimation { get; set; }
        public static Storyboard closestartanimation { get; set; }
        public StartPlaceholder()
        {
            this.InitializeComponent();
            CreatePowerOptionsMenuFlyout();
        }


        public void CreatePowerOptionsMenuFlyout()
        {
            // Create a new MenuFlyoutItem with an icon for each available power state

            FontIcon sleep = new FontIcon();
            sleep.Glyph = "\uE708";

            FontIcon hibernate = new FontIcon();
            hibernate.Glyph = "\uE823";

            FontIcon power = new FontIcon();
            power.Glyph = "\uE7E8";

            FontIcon restart = new FontIcon();
            restart.Glyph = "\uE72C";

            var sleepItem = new MenuFlyoutItem
            {
                Text = "Sleep",
                Icon = sleep,
                Tag = "PowerState.Sleep"
            };
            PowerFlyout.Items.Add(sleepItem);

            var hibernateItem = new MenuFlyoutItem
            {
                Text = "Hibernate",
                Icon = hibernate,
                Tag = "PowerState.Hibernate"
            };

            PowerFlyout.Items.Add(hibernateItem);

            var powerItem = new MenuFlyoutItem
            {
                Text = "Power",
                Icon = power,
                Tag = "PowerState.Shutdown"
            };
            PowerFlyout.Items.Add(powerItem);

            var restartItem = new MenuFlyoutItem
            {
                Text = "Restart",
                Icon = restart,
                Tag = "PowerState.Restart"
            };
            PowerFlyout.Items.Add(restartItem);










        }
    }
}
