using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.DTOs.Users;

namespace AiCalendar.MCPServer
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<UserDto> Register(LoginAndRegisterInputDto userData)
        {
            if (userData == null)
            {
                throw new Exception("User data cannot be null.");
            }

            var contentJson = JsonSerializer.Serialize(userData);

            var content = new StringContent(contentJson, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = null;

            var response = await _httpClient.PostAsync("api/v1/User/register", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var userDto = default(UserDto);

            if (response.IsSuccessStatusCode)
            {
                userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            }

            if (userDto == null)
            {
                throw new Exception("Failed to register user. Response content is null.");
            }

            return userDto;
        }

        public async Task<LoginResponseDto> Login(LoginAndRegisterInputDto userData)
        {
            if (userData == null)
            {
                throw new Exception("User data cannot be null.");
            }

            var response = await _httpClient.PostAsJsonAsync("api/v1/User/login", userData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var loginResponse = default(LoginResponseDto);
            if (response.IsSuccessStatusCode)
            {
                loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            }

            if (loginResponse == null)
            {
                throw new Exception("Failed to login user. Response content is null.");
            }

            return loginResponse;
        }

        public async Task<IEnumerable<UserDtoExtended>> GetUsers(UserFilterCriteriaDto? filter = null)
        {
            var query = new List<string>();

            if (!string.IsNullOrEmpty(filter?.Username))
                query.Add($"Username={Uri.EscapeDataString(filter.Username)}");

            if (!string.IsNullOrEmpty(filter?.Email))
                query.Add($"Email={Uri.EscapeDataString(filter.Email)}");

            if (filter?.HasActiveEvents != null)
                query.Add($"HasActiveEvents={filter.HasActiveEvents.Value}");

            var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

            var response = await _httpClient.GetAsync($"api/v1/User/users{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDtoExtended>>();

            if (users == null)
            {
                throw new Exception("Failed to retrieve users. Response content is null.");
            }

            return users;
        }

        public async Task<string> DeleteUser(string userId, string jwtToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new UnauthorizedAccessException("JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.DeleteAsync($"api/v1/User/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            return "User successfully deleted.";
        }

        public async Task<UserDto> UpdateUser(
            string userId,
            UpdateUserDto userData,
            string jwtToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("UserId cannot be null or empty.");
            }

            if (userData == null)
            {
                throw new Exception("User data cannot be null.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PutAsJsonAsync($"api/v1/User/{userId}", userData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var updatedUser = await response.Content.ReadFromJsonAsync<UserDto>();

            if (updatedUser == null)
            {
                throw new Exception("Failed to update user. Response content is null.");
            }

            return updatedUser;
        }

        public async Task<IEnumerable<EventDto>> GetUserEvents(string jwtToken, EventFilterCriteriaDto? filter)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var query = new List<string>();

            if (filter?.StartDate != null)
                query.Add($"StartDate={Uri.EscapeDataString(filter.StartDate.Value.ToString("o"))}");

            if (filter?.EndDate != null)
                query.Add($"EndDate={Uri.EscapeDataString(filter.EndDate.Value.ToString("o"))}");

            if (filter?.IsCancelled != null)
                query.Add($"IsCancelled={filter.IsCancelled.Value}");

            var queryString = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

            var response = await _httpClient.GetAsync($"api/v1/User/events/{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var events = await response.Content.ReadFromJsonAsync<IEnumerable<EventDto>>();

            if (events == null)
            {
                throw new Exception("Failed to retrieve user events. Response content is null.");
            }

            return events;
        }
    }
}
