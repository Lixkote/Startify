using Shell.Shell.ShellUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class TileGroupNameBox : UserControl
    {
        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(TextBox), null);

        public string Text
        {
            get { return BaseBox.Text; }
            set { SetValue(TextProperty, BaseBox); }
        }

        public TileGroupNameBox()
        {
            this.InitializeComponent();
        }

        private void GrabberHost_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void GrabberHost_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Hovered", true);
        }

        private void GrabberHost_GotFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Focused", true);
        }

        private void GrabberHost_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}
