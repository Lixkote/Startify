namespace WPF
{
    public class Program
    {
        [System.STAThreadAttribute()]
        public static void Main()
        {
            using (new ShellApp.App())
            {
                var app = new WPF.App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}