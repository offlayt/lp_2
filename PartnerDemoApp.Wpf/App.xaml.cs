using PartnerDemoApp.Wpf.src;
using System.Windows;

namespace PartnerDemoApp.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            AppData.EnsureCreatedAndSeed();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"Не удалось подготовить базу данных.\n{exception.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        if (e.Args.Contains("--init-only", StringComparer.OrdinalIgnoreCase))
        {
            Shutdown();
            return;
        }

        var partnersWindow = new PartnersWindow();
        MainWindow = partnersWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        partnersWindow.Show();
    }
}
