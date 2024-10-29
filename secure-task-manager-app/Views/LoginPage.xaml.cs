using secure_task_manager_app.Models;
using secure_task_manager_app.Services;

namespace secure_task_manager_app.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService = new ApiService();

    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var user = new User
        {
            Username = UsernameEntry.Text,
            Password = PasswordEntry.Text
        };

        if (await _apiService.LoginAsync(user))
        {
            await Navigation.PushAsync(new TaskListPage());
        }
        else
        {
            await DisplayAlert("Error", "Invalid credentials", "OK");
        }
    }
}
