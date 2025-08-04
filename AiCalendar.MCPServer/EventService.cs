using System.Net;
using AiCalendar.WebApi.DTOs.Event;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AiCalendar.MCPServer
{
    public class EventService
    {
        private readonly HttpClient _httpClient;

        public EventService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<EventDto> GetEventByIdAsync(string eventId)
        {
            var response = await _httpClient.GetAsync($"api/v1/Event/{eventId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response.Content}");
            }

            var eventDto = default(EventDto);
            if (response.IsSuccessStatusCode)
            {
                eventDto = await response.Content.ReadFromJsonAsync<EventDto>();
            }
            
            return eventDto;
        }

        public async Task<EventDto> CreateEventAsync(EventDto eventDto, string jwtToken)
        {
            if(eventDto == null)
            {
                throw new ArgumentNullException(nameof(eventDto), "Event data cannot be null.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new ArgumentNullException(nameof(jwtToken), "JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PostAsJsonAsync("api/v1/Event", eventDto);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Unexpected status code: {response.StatusCode}. Expected 201 Created. Response message: ${response.Content}");
            }

            return await response.Content.ReadFromJsonAsync<EventDto>();
        }

        public async Task<string> DeleteEvent(string eventId, string jwtToken)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentNullException(nameof(eventId), "EventId cannot be null.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new ArgumentNullException(nameof(jwtToken), "JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.DeleteAsync($"api/v1/Event/{eventId}");

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception($"Unexpected status code: {response.StatusCode}. Expected 204 No Content. Response message: ${response.Content}");
            }

            return "Event successfully deleted.";
        }
    }
}
