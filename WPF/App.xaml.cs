using ManagedShell;
using ManagedShell.AppBar;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using WPF.Views;

namespace WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Helpers.Launching.Shutdown shutdown;
        protected override void OnExit(ExitEventArgs e)
        {
            shutdown.Deinitialize(e);
        }
    }
}
