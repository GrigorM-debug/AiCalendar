using AiCalendar.WebApi.DTOs.Users;
using System.Net.Http.Headers;
using System.Text;

namespace AiCalendar.MCPServer
{
    public class EventParticipantsService
    {
        private readonly HttpClient _httpClient;
        public EventParticipantsService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        public async Task<IEnumerable<UserDto>> GetEventParticipantsAsync(string eventId, string jwtToken)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("EventId cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetAsync($"api/v1/EventParticipants/events/{eventId}/participants");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var participants = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();

            return participants;
        }

        public async Task<string> AddEventParticipant(
            string jwtToken, 
            string eventId, 
            string participantId)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null!");
            }

            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("EventId can not be null or empty!");
            }

            if (string.IsNullOrEmpty(participantId))
            {
                throw new Exception("ParticipantId cannot be null or empty!");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            //This need to be fixed
           var response = await _httpClient.PostAsync(
                    $"api/v1/EventParticipants/events/{eventId}/participants/{participantId}",
                    new StringContent(string.Empty, Encoding.UTF8, "application/json")
                );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            return "Participant added successfully.";
        }

        public async Task<string> RemoveEventParticipant(
            string jwtToken, 
            string eventId, 
            string participantId)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null!");
            }

            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("EventId can not be null or empty!");
            }

            if (string.IsNullOrEmpty(participantId))
            {
                throw new Exception("ParticipantId cannot be null or empty!");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.DeleteAsync(
                $"api/v1/EventParticipants/events/{eventId}/participants/{participantId}"
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            return "Participant removed successfully.";
        }
    }
}
