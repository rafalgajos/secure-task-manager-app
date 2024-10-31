using Microsoft.UI.Xaml;

namespace secure_task_manager_app.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var mainWindow = Microsoft.Maui.MauiWinUIApplication.Current.MainWindow;

        // Monitoring application window activity
        mainWindow.Activated += (s, e) =>
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
            {
                Console.WriteLine("The application is inactive - alerting or hiding sensitive data.");
            }
        };
    }
}
