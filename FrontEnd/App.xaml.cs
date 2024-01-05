using Windows.UI.Xaml;

namespace ShellApp
{
    public sealed partial class App : Microsoft.Toolkit.Win32.UI.XamlHost.XamlApplication
    {
        public App()
        {
            this.Initialize();
            (Window.Current as object as IWindowPrivate).TransparentBackground = true;
        }
    }
}
