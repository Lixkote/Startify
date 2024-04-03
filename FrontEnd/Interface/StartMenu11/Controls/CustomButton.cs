using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Shell.Interface.StartMenu11.Controls
{
    public class CustomButton : Button
    {
        public CustomButton()
        {
            this.DefaultStyleKey = typeof(Button);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            // Check if the pointer event was triggered by left mouse button
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse &&
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // Pass the pointer event to the base class to handle click event
                base.OnPointerPressed(e);
            }
            else
            {
                // For non-left mouse button events (e.g., right-click), show context menu
                // ShowContextMenu();
            }
        }
    }
}

