using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System;
using Microsoft.Maui.Controls;

namespace secure_task_manager_app.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly ApiService _apiService;

        public LoginPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }

            var user = new User
            {
                Username = username,
                Password = password
            };

            if (await _apiService.LoginAsync(user))
            {
                await Navigation.PopAsync(); // Return to previous page after successful login
                MessagingCenter.Send(this, "SyncTasks"); // Send message to TaskListPage to synchronize tasks
            }
            else
            {
                await DisplayAlert("Error", "Login failed. Could not synchronize tasks.", "OK");
            }
        }

        // Support for redirection to the registration page
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage()); // Redirect to the registration page
        }
    }
}
