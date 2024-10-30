using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using secure_task_manager_app.Models;

namespace secure_task_manager_app.Services
{
    public class ApiService
    {
        private static readonly HttpClient _httpClient;
        private static string _jwtToken; // Zmieniona na statyczną, aby zapewnić trwałość między wywołaniami metod

        static ApiService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://127.0.0.1:8443")
            };
        }

        private void SetJwtToken(string jwtToken)
        {
            _jwtToken = jwtToken;
            Console.WriteLine($"_jwtToken in SetJwtToken: {_jwtToken}");
        }

        // Metoda rejestracji użytkownika
        public async Task<bool> RegisterAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("/register", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("/login", user);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                string token = result.token;

                SetJwtToken(token);
                Console.WriteLine($"_jwtToken after LoginAsync: {_jwtToken}");
                return true;
            }
            Console.WriteLine("Login failed");
            return false;
        }

        public async Task<List<Models.Task>> GetTasksAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_jwtToken))
                {
                    Console.WriteLine("Token missing or expired, re-login required.");
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/tasks");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                Console.WriteLine($"Authorization Header in GetTasksAsync: {request.Headers.Authorization}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Models.Task>>();
                }
                else
                {
                    Console.WriteLine($"Error fetching tasks: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching tasks: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> AddTaskAsync(Models.Task task)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/tasks")
            {
                Content = JsonContent.Create(task)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTaskAsync(Models.Task task)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/tasks/{task.Id}")
            {
                Content = JsonContent.Create(task)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/tasks/{taskId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
