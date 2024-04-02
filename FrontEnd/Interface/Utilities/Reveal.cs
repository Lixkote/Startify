using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.Foundation;

namespace Shell.Shell.ShellUtils
{
    public sealed partial class Reveal : UserControl
    {
        public Reveal()
        {
            DefaultStyleKey = typeof(Reveal);

            PointerMoved += RootVisual_PointerMoved;
            PointerExited += RootVisual_PointerLeave;
        }

        public static readonly DependencyProperty GradientStopsProperty =
            DependencyProperty.Register("GradientStops", typeof(GradientStopCollection), typeof(Reveal), null);

        public GradientStopCollection GradientStops
        {
            get => (GradientStopCollection)GetValue(GradientStopsProperty);
            set => SetValue(GradientStopsProperty, value);
        }

        public static readonly DependencyProperty TargetButtonProperty =
            DependencyProperty.Register("TargetButton", typeof(Button), typeof(Reveal), null);

        public Button TargetButton
        {
            get => (Button)GetValue(TargetButtonProperty);
            set => SetValue(TargetButtonProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        void RootVisual_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (TargetButton != null)
            {
                var curPos = e.GetCurrentPoint(TargetButton).Position;

                double curX = curPos.X / 5;
                double curY = curPos.Y / 5;

                var newBrush = new LinearGradientBrush();
                newBrush.StartPoint = new Point(curX, curY);
                newBrush.EndPoint = new Point(TargetButton.ActualWidth + curX, TargetButton.ActualHeight + curY);

                // Use the provided GradientStops
                newBrush.GradientStops = GradientStops;

                // Update the BorderBrush of the specified TargetButton
                TargetButton.BorderBrush = newBrush;
            }
        }

        void RootVisual_PointerLeave(object sender, PointerRoutedEventArgs e)
        {
            if (TargetButton != null)
            {
                // Reset the BorderBrush when the mouse leaves
                TargetButton.BorderBrush = null;
            }
        }
    }
}
