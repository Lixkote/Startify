using Shell.Shell.ShellUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Xml;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using static Shell.Shell.ShellUtils.StartMenuSelector;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ShellApp.Shell.Start
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public partial class StartPlaceholder : UserControl
    {
        // Declare the event
        public event EventHandler<ItemClickEventArgs> DirectoryChildClicked;
        public event EventHandler<ItemClickEventArgs> ErrorHappened;

        public event EventHandler<RoutedEventArgs> UninstallSettingsShouldOpen;
        public event EventHandler<RoutedEventArgs> RunAsAdminClicked;
        public event EventHandler<RoutedEventArgs> OpenFileLocationClicked;
        public event EventHandler<RoutedEventArgs> exitstartapp;


        public event EventHandler AnimationStarted;
        public StartPlaceholder()
        {
            this.InitializeComponent();
            this.Loaded += Start_Loaded;
            this.ErrorHappened += errornotif;
        }

        private void errornotif(object sender, ItemClickEventArgs e)
        {

        }

        private bool IsFolderOpened = false;

        public void StartCloseStartAnimation()
        {
            closestartanimation.Begin();
            AnimationStarted?.Invoke(this, EventArgs.Empty);
        }

        public void StartOpenStartAnimation()
        {
            openstartanimation.Begin();
            AnimationStarted?.Invoke(this, EventArgs.Empty);
        }

        private async void Start_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IReadOnlyList<User> users = await User.FindAllAsync();

            var current = users.Where(p => p.AuthenticationStatus == UserAuthenticationStatus.LocallyAuthenticated &&
                                        p.Type == UserType.LocalUser).FirstOrDefault();

            // user may have picture
            var picture = await current.GetPictureAsync(UserPictureSize.Size64x64);
            if (picture != null)
            {
                var imageStream = await picture.OpenReadAsync();
                // Create a BitmapImage object
                BitmapImage bitmapImage = new BitmapImage();

                // Set the source of the BitmapImage from the stream
                await bitmapImage.SetSourceAsync(imageStream);
                UserAV.Source = bitmapImage;
            }
            // Get the current theme of the app
            var currentTheme = Application.Current.RequestedTheme;
            if (IsColorizationEnabled.Text == "1" && currentTheme == ApplicationTheme.Dark)
            {
                try
                {
                    // Get the current system accent color
                    var uiSettings = new UISettings();
                    var accentColor = uiSettings.GetColorValue(UIColorType.AccentDark2);

                    // Set the TintColor and FallbackColor of the AcrylicBrush to the system accent color
                    ((AcrylicBrush)Resources["halal"]).TintColor = accentColor;
                    ((AcrylicBrush)Resources["halal"]).FallbackColor = accentColor;

                    // Set the background of the startbackground element to the modified AcrylicBrush
                    startbackground.Background = (AcrylicBrush)Resources["halal"];
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error applying accent color" + ex.Message);
                }
            }
            // Cool acrylic demo
            // startbackground.Background = (AcrylicBrush)Resources["CustomAcrylicInAppLuminosity"];

        }

        private async void SettingsButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:"));
        }

        private async void ImagesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the user's pictures folder
            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;

            // Launch the folder in the default file explorer app
            await Launcher.LaunchFolderAsync(picturesFolder);
        }

        private async void DocumentsButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Get the user's pictures folder
            StorageFolder docsFolder = KnownFolders.DocumentsLibrary;

            // Launch the folder in the default file explorer app
            await Launcher.LaunchFolderAsync(docsFolder);

        }

        private void DirectoryChildContainer_ItemClick(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            DirectoryChildClicked?.Invoke(sender, e);
        }
        private void UninstallAppMenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            UninstallSettingsShouldOpen?.Invoke(sender, e);
        }

        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            OpenFileLocationClicked?.Invoke(sender, e);
        }

        private void RunAsAdminItem_Click(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            RunAsAdminClicked?.Invoke(sender, e);
        }

        private void PinToTaskbar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PinToStartifyMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitStartify_Click(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            exitstartapp?.Invoke(sender, e);
        }
    }
}
