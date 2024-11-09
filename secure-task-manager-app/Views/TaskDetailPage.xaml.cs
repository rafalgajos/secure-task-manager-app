using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System.Collections.ObjectModel;
using System;

namespace secure_task_manager_app.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly SQLiteService _sqliteService;
        private readonly ObservableCollection<Models.Task> _tasks; // Dodaj referencję do kolekcji zadań
        public secure_task_manager_app.Models.Task Task { get; set; }
        public bool IsEditMode => Task.Id != 0;

        public TaskDetailPage(Models.Task task, ObservableCollection<Models.Task> tasks)
        {
            InitializeComponent();
            _sqliteService = new SQLiteService(); // Inicjalizacja SQLiteService
            Task = task;
            _tasks = tasks; // Przypisz kolekcję zadań
            BindingContext = this;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Zapisz zadanie do lokalnej bazy danych
                await _sqliteService.SaveTaskAsync(Task);

                // Znajdź zadanie w liście zadań i zaktualizuj je
                if (_tasks != null)
                {
                    var existingTaskIndex = _tasks.IndexOf(Task);
                    if (existingTaskIndex >= 0)
                    {
                        _tasks[existingTaskIndex] = Task; // Zaktualizuj istniejące zadanie w kolekcji
                    }
                    else
                    {
                        _tasks.Add(Task); // Dodaj zadanie, jeśli go jeszcze nie ma w kolekcji
                    }
                }

                await Navigation.PopAsync(); // Powrót do poprzedniej strony
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
                    _tasks.Remove(Task); // Usuń zadanie z kolekcji
                }
            }

            await Navigation.PopAsync(); // Powrót do poprzedniej strony
        }
    }
}
