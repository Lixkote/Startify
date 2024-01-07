using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Szablon elementu Kontrolka użytkownika jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234236

namespace Shell.Interface.StartMenu
{
    public sealed partial class Tile : UserControl
    {
        public event EventHandler<RoutedEventArgs> Click;
        public event EventHandler<RoutedEventArgs> UnpinTile;

        // Helper method to find the parent of a specific type
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }
        public Tile()
        {
            this.InitializeComponent();

        }
        // Enum for Tile Sizes
        public enum TileSize
        {
            Small,
            Medium,
            Wide,
            Large
        }


        private void TileMain_Loaded(object sender, RoutedEventArgs e)
        {
            // CustomBacking
            if (!string.IsNullOrEmpty(TileColorInternal.Text))
            {
                if (TileColorInternal.Text.StartsWith("#") && TileColorInternal.Text.Length == 7)
                {
                    try
                    {
                        // Convert hex color string to Color object
                        Windows.UI.Color color = Windows.UI.ColorHelper.FromArgb(
                            255,
                            Convert.ToByte(TileColorInternal.Text.Substring(1, 2), 16),
                            Convert.ToByte(TileColorInternal.Text.Substring(3, 2), 16),
                            Convert.ToByte(TileColorInternal.Text.Substring(5, 2), 16));

                        TileButton.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
                        TileButton.Background = new Windows.UI.Xaml.Media.SolidColorBrush(color);
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions (e.g., invalid hex color string)
                        Debug.WriteLine($"Error setting background color: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        // Load image from path
                        Uri uri = new Uri(TileColorInternal.Text, UriKind.RelativeOrAbsolute);
                        Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage(uri);

                        // Set TileButton background using the image
                        // TileButton.Style = null; // Clear any previous style
                        TileButton.Background = new Windows.UI.Xaml.Media.ImageBrush
                        {
                            ImageSource = bitmapImage,
                            Stretch = Windows.UI.Xaml.Media.Stretch.UniformToFill // Adjust as needed
                        };
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions (e.g., invalid image path)
                        Debug.WriteLine($"Error setting background image: {ex.Message}");
                    }
                }
            }
            else
            {
                // Do nothing if TileColorInternal.Text is null or empty
            }

            // Default Tile Size
            TileMain.Width = 96;
            TileMain.Height = 96;

            // TileSizes using Enum
            if (Enum.TryParse(TileSizeInternal.Text, out TileSize tileSize))
            {
                switch (tileSize)
                {
                    case TileSize.Small:
                        TileMain.Width = 46;
                        TileMain.Height = 46;
                        TileDisplayName.Visibility = Visibility.Collapsed;
                        TileSizeSmallContextMenuItem.IsChecked = true;
                        TileSizeMediumContextMenuItem.IsChecked = false;
                        TileSizeWideContextMenuItem.IsChecked = false;
                        TileSizeLargeContextMenuItem.IsChecked = false;
                        break;
                    case TileSize.Medium:
                        TileSizeSmallContextMenuItem.IsChecked = false;
                        TileSizeMediumContextMenuItem.IsChecked = true;
                        TileSizeWideContextMenuItem.IsChecked = false;
                        TileSizeLargeContextMenuItem.IsChecked = false;
                        break; // Already set to default size
                    case TileSize.Wide:
                        TileMain.Width = 196;
                        TileMain.Height = 96;
                        TileSizeSmallContextMenuItem.IsChecked = false;
                        TileSizeMediumContextMenuItem.IsChecked = false;
                        TileSizeWideContextMenuItem.IsChecked = true;
                        TileSizeLargeContextMenuItem.IsChecked = false;
                        break;
                    case TileSize.Large:
                        TileMain.Width = 196;
                        TileMain.Height = 196;
                        TileSizeSmallContextMenuItem.IsChecked = false;
                        TileSizeMediumContextMenuItem.IsChecked = false;
                        TileSizeWideContextMenuItem.IsChecked = false;
                        TileSizeLargeContextMenuItem.IsChecked = true;
                        break;
                    default:
                        break; // Default size
                }
            }
        }

        private void TileSizeSmallContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TileMain.Width = 46;
            TileMain.Height = 46;
            TileDisplayName.Visibility = Visibility.Collapsed;
            TileSizeSmallContextMenuItem.IsChecked = true;
            TileSizeMediumContextMenuItem.IsChecked = false;
            TileSizeWideContextMenuItem.IsChecked = false;
            TileSizeLargeContextMenuItem.IsChecked = false;
        }

        private void TileSizeMediumContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TileMain.Width = 96;
            TileMain.Height = 96;
            TileDisplayName.Visibility = Visibility.Visible;
            TileSizeSmallContextMenuItem.IsChecked = false;
            TileSizeMediumContextMenuItem.IsChecked = true;
            TileSizeWideContextMenuItem.IsChecked = false;
            TileSizeLargeContextMenuItem.IsChecked = false;
        }

        private void TileSizeWideContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TileMain.Width = 196;
            TileMain.Height = 96;
            TileDisplayName.Visibility = Visibility.Visible;
            TileSizeSmallContextMenuItem.IsChecked = false;
            TileSizeMediumContextMenuItem.IsChecked = false;
            TileSizeWideContextMenuItem.IsChecked = true;
            TileSizeLargeContextMenuItem.IsChecked = false;
        }

        private void TileSizeLargeContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TileMain.Width = 196;
            TileMain.Height = 196;
            TileDisplayName.Visibility = Visibility.Visible;
            TileSizeSmallContextMenuItem.IsChecked = false;
            TileSizeMediumContextMenuItem.IsChecked = false;
            TileSizeWideContextMenuItem.IsChecked = false;
            TileSizeLargeContextMenuItem.IsChecked = true;
        }
        private bool isDragging = false;

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            string PathUniversal = "Classic Path:" + PathClassic.Text + "Immersive Path:" + PathImmersive.Text;
            Click?.Invoke(PathUniversal, e);
        }

        private void TileMain_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            string PathUniversal = "Classic Path:" + PathClassic.Text + "Immersive Path:" + PathImmersive.Text;
            Click?.Invoke(PathUniversal, e);
        }

        private void TileButton_Holding(object sender, HoldingRoutedEventArgs e)
        {

        }

        private void TileButton_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            TileButton.IsHitTestVisible = false;
        }

        private void TileButton_DragLeave(object sender, DragEventArgs e)
        {
            TileButton.IsHitTestVisible = true;
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            string TileGroupTileName = PathClassic.Text;
            UnpinTile?.Invoke(TileGroupTileName, e);
        }
    }
}
