using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

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
            this.Loaded += Start_Loaded;
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
    }
}
