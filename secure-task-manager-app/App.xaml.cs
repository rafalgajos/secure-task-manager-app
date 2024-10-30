using secure_task_manager_app.Views;

namespace secure_task_manager_app;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());
    }
}
