using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices.Sensors;
using System;

namespace secure_task_manager_app.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly SQLiteService _sqliteService;
        private readonly ApiService _apiService;
        private readonly ObservableCollection<Models.Task> _tasks;
        public secure_task_manager_app.Models.Task Task { get; set; }
        public bool IsEditMode => Task.Id != 0;

        public TaskDetailPage(Models.Task task, ObservableCollection<Models.Task> tasks)
        {
            InitializeComponent();
            _sqliteService = new SQLiteService(App.DatabasePassword);
            _apiService = new ApiService();
            Task = task;
            _tasks = tasks;
            BindingContext = this;
        }

        private async void OnGetLocationClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Cannot access location", "OK");
                    return;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    Task.Location = $"{location.Latitude}, {location.Longitude}";
                    OnPropertyChanged(nameof(Task.Location)); // Update the UI
                }
                else
                {
                    await DisplayAlert("Location Error", "Could not retrieve location", "OK");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Error", "Location not supported on this device", "OK");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Error", "Location services are not enabled", "OK");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Error", "Permission to access location was denied", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to get location: {ex.Message}", "OK");
            }
        }



        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (Task.DueDate == null)
                {
                    Task.DueDate = DateTime.MinValue; // lub inną domyślną wartość
                }

                await _sqliteService.SaveTaskAsync(Task);

                if (_tasks != null)
                {
                    var existingTaskIndex = _tasks.IndexOf(Task);
                    if (existingTaskIndex >= 0)
                    {
                        _tasks[existingTaskIndex] = Task;
                    }
                    else
                    {
                        _tasks.Add(Task);
                    }
                }

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Wystąpił błąd podczas zapisu: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                // Pytamy użytkownika, czy usunąć zadanie również z serwera
                bool deleteFromServer = await DisplayAlert("Potwierdzenie", "Czy chcesz również usunąć to zadanie z serwera?", "Tak", "Nie");

                if (deleteFromServer)
                {
                    // Usuń zadanie z serwera
                    bool result = await _apiService.DeleteTaskAsync(Task.Id);
                    if (!result)
                    {
                        await DisplayAlert("Błąd", "Nie udało się usunąć zadania z serwera.", "OK");
                        return;
                    }
                }

                // Usuń zadanie z lokalnej bazy danych
                await _sqliteService.DeleteTaskAsync(Task);

                // Usuń zadanie z listy interfejsu, jeśli tam istnieje
                if (_tasks != null && _tasks.Contains(Task))
                {
                    _tasks.Remove(Task);
                }
            }

            // Powrót do poprzedniej strony po usunięciu zadania
            await Navigation.PopAsync();
        }

    }
}
