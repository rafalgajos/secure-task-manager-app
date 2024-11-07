using secure_task_manager_app.Models;
using secure_task_manager_app.Services;
using System;

namespace secure_task_manager_app.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly SQLiteService _sqliteService;
        private readonly Action _refreshTaskListAction;
        public secure_task_manager_app.Models.Task Task { get; set; }
        public bool IsEditMode => Task.Id != 0;

        public TaskDetailPage(Models.Task task, Action refreshTaskListAction)
        {
            InitializeComponent();
            _sqliteService = new SQLiteService(); // Zamiast używania using, używamy obiektu bezpośrednio.
            Task = task;
            _refreshTaskListAction = refreshTaskListAction;
            BindingContext = this;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _refreshTaskListAction?.Invoke();
            await Navigation.PopAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                await _sqliteService.DeleteTaskAsync(Task);
            }

            _refreshTaskListAction?.Invoke();
            await Navigation.PopAsync();
        }
    }
}
