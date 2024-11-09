using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System.Collections.ObjectModel;
using System;

namespace secure_task_manager_app.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly SQLiteService _sqliteService;
        private readonly ObservableCollection<Models.Task> _tasks; // Kolekcja zadań
        public secure_task_manager_app.Models.Task Task { get; set; }
        public bool IsEditMode => Task.Id != 0;

        public TaskDetailPage(Models.Task task, ObservableCollection<Models.Task> tasks)
        {
            InitializeComponent();
            _sqliteService = new SQLiteService(App.DatabasePassword); // Przekazanie hasła do konstruktora
            Task = task;
            _tasks = tasks;
            BindingContext = this;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
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
                await _sqliteService.DeleteTaskAsync(Task);
                if (_tasks != null && _tasks.Contains(Task))
                {
                    _tasks.Remove(Task);
                }
            }

            await Navigation.PopAsync();
        }
    }
}
