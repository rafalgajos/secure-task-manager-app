using System;
using System.Net.Http.Json;
using secure_task_manager_app.Models;
using Newtonsoft.Json;

namespace secure_task_manager_app.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://127.0.0.1:8443"; // URL backendu

        public ApiService()
        {
            // Konfiguracja ignorowania certyfikatu SSL
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://127.0.0.1:8443")
            };
        }

        // Logowanie użytkownika
        public async Task<bool> LoginAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/login", user);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                user.Token = JsonConvert.DeserializeObject<dynamic>(content).token;
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);
                return true;
            }
            return false;
        }

        // Pobieranie listy zadań
        public async Task<List<Models.Task>> GetTasksAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/tasks");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Models.Task>>();
            }
            return null;
        }

        // Dodawanie nowego zadania
        public async Task<bool> AddTaskAsync(Models.Task task)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/tasks", task);
            return response.IsSuccessStatusCode;
        }

        // Aktualizacja zadania
        public async Task<bool> UpdateTaskAsync(Models.Task task)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/tasks/{task.Id}", task);
            return response.IsSuccessStatusCode;
        }

        // Usuwanie zadania
        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/tasks/{taskId}");
            return response.IsSuccessStatusCode;
        }
    }
}

