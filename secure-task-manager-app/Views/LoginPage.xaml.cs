using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System;

namespace secure_task_manager_app.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();

        public LoginPage()
        {
            InitializeComponent();
        }

        // Po udanym logowaniu na LoginPage
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var user = new User
            {
                Username = UsernameEntry.Text,
                Password = PasswordEntry.Text
            };

            bool isLoggedIn = await _apiService.LoginAsync(user);
            if (isLoggedIn)
            {
                // Przejdź do TaskListPage, gdzie zostanie wywołane GetTasksAsync
                await Navigation.PushAsync(new TaskListPage());
            }
            else
            {
                await DisplayAlert("Error", "Invalid username or password", "OK");
            }
        }


        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
