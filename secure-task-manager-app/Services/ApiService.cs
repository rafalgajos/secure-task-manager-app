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

        static ApiService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
            // Wymaga pełnej weryfikacji SSL (należy usunąć wszelkie wyłączenia weryfikacji SSL w środowisku produkcyjnym)
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://127.0.0.1:8443")
            };
        }

        private async System.Threading.Tasks.Task SetJwtToken(string jwtToken)
        {
            // Zapisz token JWT w zaszyfrowanym pliku lokalnym
            await SecureTokenStorage.SaveTokenAsync(jwtToken);
        }

        private async Task<string> GetJwtToken()
        {
            // Odczytaj token JWT z zaszyfrowanego pliku lokalnego
            return await SecureTokenStorage.GetTokenAsync();
        }

        private void ClearJwtToken()
        {
            // Usuń token JWT z zaszyfrowanego pliku lokalnego
            SecureTokenStorage.ClearToken();
        }

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

                await SetJwtToken(token); // Zapis tokenu w zaszyfrowanym pliku
                Console.WriteLine("Login successful and JWT token stored securely.");
                return true;
            }

            Console.WriteLine("Login failed");
            return false;
        }

        public async Task<List<Models.Task>> GetTasksAsync()
        {
            try
            {
                var token = await GetJwtToken(); // Odczyt tokenu z zaszyfrowanego pliku

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token missing or expired, re-login required.");
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/tasks");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var token = await GetJwtToken();

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token missing or expired, re-login required.");
                return false;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/tasks")
            {
                Content = JsonContent.Create(task)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTaskAsync(Models.Task task)
        {
            var token = await GetJwtToken();

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token missing or expired, re-login required.");
                return false;
            }

            var request = new HttpRequestMessage(HttpMethod.Put, $"/tasks/{task.Id}")
            {
                Content = JsonContent.Create(task)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var token = await GetJwtToken();

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token missing or expired, re-login required.");
                return false;
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/tasks/{taskId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public void Logout()
        {
            ClearJwtToken(); // Czyszczenie tokenu przy wylogowaniu
            Console.WriteLine("User logged out and JWT token removed from secure storage.");
        }
    }
}
