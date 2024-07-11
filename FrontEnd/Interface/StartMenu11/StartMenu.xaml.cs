using Shell.Shell.ShellUtils;
using Shell.Interface.StartMenu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using static Shell.Shell.ShellUtils.StartMenuSelector;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Shell.Interface.StartMenu11
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public partial class StartMenu : UserControl
    {
        // Declare the event
        public event EventHandler<ItemClickEventArgs> DirectoryChildClicked;
        public event EventHandler<RoutedEventArgs> TileClickedMain;
        public event EventHandler<RoutedEventArgs> TilePinnedMain;
        public event EventHandler<RoutedEventArgs> TileGroupRenamedMain;
        public event EventHandler<RoutedEventArgs> TileUnpinnedMain;
        public event EventHandler<ItemClickEventArgs> ErrorHappened;

        public event EventHandler<RoutedEventArgs> UninstallSettingsShouldOpen;
        public event EventHandler<RoutedEventArgs> RunAsAdminClicked;
        public event EventHandler<RoutedEventArgs> OpenFileLocationClicked;
        public event EventHandler<RoutedEventArgs> exitstartapp;
        Brush originalBackground;


        public event EventHandler AnimationStarted;
        public StartMenu()
        {
            this.InitializeComponent();
            this.Loaded += Start_Loaded;
            this.ErrorHappened += errornotif;
        }

        private void errornotif(object sender, ItemClickEventArgs e)
        {

        }

        private bool IsFolderOpened = false;

        private Storyboard currentAnimation;

        public void EnableDockedDesign()
        {
            StartMenuAcrylicOverlay.Visibility = Visibility.Collapsed;
            startbackground.Margin = new Windows.UI.Xaml.Thickness(0);
            startbackground.CornerRadius = new CornerRadius(0,10,0,0);
            startbackground.BorderThickness = new Thickness(0,1,1,0);
        }

        public Task ToggleStartAnimation()
        {
            if (currentAnimation != null)
            {
                // If an animation is currently executing, reverse it
                currentAnimation.Stop();
                currentAnimation.AutoReverse = !currentAnimation.AutoReverse;
                currentAnimation.Begin();
            }
            else
            {
                // Otherwise, start the appropriate animation
                if (Visibility == Visibility.Visible)
                {
                    return StartCloseStartAnimation();
                }
                else
                {
                    return StartOpenStartAnimation();
                }
            }

            return Task.CompletedTask;
        }

        public Task StartCloseStartAnimation()
        {
            currentAnimation = closestartanimation;
            return StartAnimation(closestartanimation, "closestartanimation");
        }

        public Task StartOpenStartAnimation()
        {
            currentAnimation = openstartanimation;
            return StartAnimation(openstartanimation, "openstartanimation");
        }

        private Task StartAnimation(Storyboard animation, string animationName)
        {
            TaskCompletionSource<bool> animationCompletionSource = new TaskCompletionSource<bool>();

            try
            {
                // Ensure that the animation is properly initialized.
                if (animation != null)
                {
                    animation.Completed += (sender, args) =>
                    {
                        animationCompletionSource.TrySetResult(true);
                    };

                    animation.Begin();
                }
                else
                {
                    animationCompletionSource.TrySetException(new InvalidOperationException($"{animationName} not properly initialized."));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                animationCompletionSource.TrySetException(ex);
            }

            return animationCompletionSource.Task;
        }



        private async void Start_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IReadOnlyList<User> users = await User.FindAllAsync();

            var current = users.Where(p => p.AuthenticationStatus == UserAuthenticationStatus.LocallyAuthenticated &&
                                        p.Type == UserType.LocalUser).FirstOrDefault();

            // user may have picture
            var picture = await current.GetPictureAsync(UserPictureSize.Size64x64);
            string username = current.ToString();
            if (picture != null)
            {
                try
                {
                    var imageStream = await picture.OpenReadAsync();
                    // Create a BitmapImage object
                    BitmapImage bitmapImage = new BitmapImage();

                    // Set the source of the BitmapImage from the stream
                    await bitmapImage.SetSourceAsync(imageStream);
                    UserAV.Source = bitmapImage;
                }
                catch
                {

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

        private void PinToTaskbar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitStartify_Click(object sender, RoutedEventArgs e)
        {
            // Call the OnDirectoryChildClicked method to raise the event
            exitstartapp?.Invoke(sender, e);
        }
        public async void AccentWasEnabled()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Get the current system accent color
                    var uiSettings = new UISettings();
                    var accentColor = uiSettings.GetColorValue(UIColorType.AccentDark2);

                    // Set the TintColor and FallbackColor of the AcrylicBrush to the system accent color
                    ((AcrylicBrush)Resources["halal"]).TintColor = accentColor;
                    ((AcrylicBrush)Resources["halal"]).FallbackColor = accentColor;

                    // Set the background of the startbackground element to the modified AcrylicBrush
                    startbackground.Background = (AcrylicBrush)Resources["halal"];
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error applying accent color: " + ex.Message);
            }
        }

        public async void AccentWasDisabled()
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    startbackground.Background = originalBackground;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error disabling accent color: " + ex.ToString());
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void LargeSizeMenuItem2_Click(object sender, RoutedEventArgs e)
        {
            GridViewItem targetitem = sender as GridViewItem;
            if (targetitem != null)
            {
                // Assuming "Tile1" is defined in XAML as a DataTemplate
                targetitem.ContentTemplate = this.Resources["Tile2"] as DataTemplate;
            }
        }

        private void SmallSizeMenuItem2_Click(object sender, RoutedEventArgs e)
        {
            GridViewItem targetitem = sender as GridViewItem;
            if (targetitem != null)
            {
                // Assuming "Tile1" is defined in XAML as a DataTemplate
                targetitem.ContentTemplate = this.Resources["Tile1"] as DataTemplate;
            }
        }

        public void TileGroupCTRL_TileClicked(object sender, RoutedEventArgs e)
        {
            TileClickedMain?.Invoke(sender, e);
        }

        private void TileGroupCTRL_TileUnpinned(object sender, RoutedEventArgs e)
        {

            TileUnpinnedMain?.Invoke(sender, e);
        }

        private void TileGroupCTRL_TileGroupNameChanged(object sender, RoutedEventArgs e)
        {
            TileGroupRenamedMain?.Invoke(sender, e);
        }
    }
}
