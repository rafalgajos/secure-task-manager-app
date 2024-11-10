using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System.Collections.ObjectModel;
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
                bool deleteFromServer = await DisplayAlert("Potwierdzenie", "Czy chcesz również usunąć to zadanie z serwera?", "Tak", "Nie");

                // Usuń zadanie lokalnie
                await _sqliteService.DeleteTaskAsync(Task);

                // Usuń zadanie z listy interfejsu, jeśli jest tam obecne
                if (_tasks != null && _tasks.Contains(Task))
                {
                    _tasks.Remove(Task);
                }

                // Jeśli użytkownik chce usunąć zadanie z serwera, wyślij żądanie DELETE
                if (deleteFromServer)
                {
                    bool result = await _apiService.DeleteTaskAsync(Task.Id);
                    if (!result)
                    {
                        await DisplayAlert("Błąd", "Nie udało się usunąć zadania z serwera.", "OK");
                    }
                }
            }

            await Navigation.PopAsync();
        }
    }
}
