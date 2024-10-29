using secure_task_manager_app.Models;
using secure_task_manager_app.Services;

namespace secure_task_manager_app.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        private readonly ApiService _apiService;
        public secure_task_manager_app.Models.Task Task { get; set; }
        public bool IsEditMode => Task.Id != 0;

        public TaskDetailPage(Models.Task task)
        {
            InitializeComponent();
            _apiService = new ApiService();
            Task = task;
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
            await Navigation.PopAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                await _apiService.DeleteTaskAsync(Task.Id);
            }
            await Navigation.PopAsync();
        }
    }
}
