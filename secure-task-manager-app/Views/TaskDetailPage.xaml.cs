using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System;

namespace secure_task_manager_app.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly Action _refreshTaskListAction;

        public secure_task_manager_app.Models.Task Task { get; set; }
        public bool IsEditMode => Task.Id != 0;

        public TaskDetailPage(Models.Task task, Action refreshTaskListAction)
        {
            InitializeComponent();
            _apiService = new ApiService();
            Task = task;
            _refreshTaskListAction = refreshTaskListAction;
            BindingContext = this;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                await _apiService.UpdateTaskAsync(Task);
            }
            else
            {
                await _apiService.AddTaskAsync(Task);
            }

            // Wywołanie metody odświeżającej listę zadań
            _refreshTaskListAction?.Invoke();

            await Navigation.PopAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                await _apiService.DeleteTaskAsync(Task.Id);
            }

            // Odświeżenie listy po usunięciu zadania
            _refreshTaskListAction?.Invoke();

            await Navigation.PopAsync();
        }
    }
}
