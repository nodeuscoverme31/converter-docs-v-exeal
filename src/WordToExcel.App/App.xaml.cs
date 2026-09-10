using System.Windows;

namespace WordToExcel.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Any(arg => string.Equals(arg, "--bootstrap-smoke", StringComparison.OrdinalIgnoreCase)))
        {
            Shutdown(0);
            return;
        }

        base.OnStartup(e);

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }
}
