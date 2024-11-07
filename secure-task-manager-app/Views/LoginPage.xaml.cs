using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System;

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
                await DisplayAlert("Błąd", "Podaj nazwę użytkownika i hasło", "OK");
                return;
            }

            var user = new User
            {
                Username = username,
                Password = password
            };

            if (await _apiService.LoginAsync(user))
            {
                await Navigation.PopAsync(); // Wróć do poprzedniej strony po udanym logowaniu
                MessagingCenter.Send(this, "SyncTasks"); // Wyślij wiadomość do TaskListPage, aby zsynchronizować zadania
            }
            else
            {
                await DisplayAlert("Błąd", "Logowanie nieudane. Nie udało się zsynchronizować zadań.", "OK");
            }
        }
    }
}
