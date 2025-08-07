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
            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("EventId cannot be null or empty.");
            }

            var response = await _httpClient.GetAsync($"api/v1/Event/{eventId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var eventDto = default(EventDto);
            if (response.IsSuccessStatusCode)
            {
                eventDto = await response.Content.ReadFromJsonAsync<EventDto>();
            }

            return eventDto;
        }

        public async Task<EventDto> CreateEventAsync(CreateEventDto eventDto, string jwtToken)
        {
            if (eventDto == null)
            {
                throw new Exception("Event data cannot be null.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PostAsJsonAsync("api/v1/Event", eventDto);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 201 Created. Response message: ${response}");
            }

            return await response.Content.ReadFromJsonAsync<EventDto>();
        }

        public async Task<string> DeleteEvent(string eventId, string jwtToken)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("EventId cannot be null.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.DeleteAsync($"api/v1/Event/{eventId}");

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 204 No Content. Response message: ${response}");
            }

            return "Event successfully deleted.";
        }

        public async Task<EventDto> CancelEventAsync(string eventId, string jwtToken)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("EventId can not be null!");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token can not be null!");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PatchAsync($"api/v1/Event/{eventId}/cancel", null);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200 OK. Response message: ${response}");
            }

            var eventDto = await response.Content.ReadFromJsonAsync<EventDto>();

            if (eventDto == null)
            {
                throw new Exception("Failed to retrieve the updated event after cancellation.");
            }

            return eventDto;
        }

        public async Task<IEnumerable<EventDto>> GetAllEventsAsync(EventFilterCriteriaDto? filter = null)
        {
            var query = new List<string>();

            if (filter?.StartDate != null)
            {
                query.Add($"StartDate={WebUtility.UrlEncode(filter.StartDate.Value.ToString("o"))}");
            }
            if (filter?.EndDate != null)
            {
                query.Add($"EndDate={WebUtility.UrlEncode(filter.EndDate.Value.ToString("o"))}");
            }
            if (filter?.IsCancelled != null)
            {
                query.Add($"IsCancelled={filter.IsCancelled.Value}");
            }

            var queryString = query.Any() ? "?" + string.Join("&", query) : "";

            var response = await _httpClient.GetAsync($"api/v1/Event{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            var events = await response.Content.ReadFromJsonAsync<IEnumerable<EventDto>>();

            if (events == null)
            {
                throw new Exception("Failed to retrieve events.");
            }

            return events;
        }

        public async Task<EventDto> UpdateEventAsync(string eventId, UpdateEventDto eventDto, string jwtToken)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new Exception("EventId cannot be null or empty.");
            }

            if (eventDto == null)
            {
                throw new Exception("Event data cannot be null.");
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new Exception("JWT token cannot be null.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PutAsJsonAsync($"api/v1/Event/{eventId}", eventDto);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Unexpected status code: {response.StatusCode}. Expected 200. Response message: ${response}");
            }

            return await response.Content.ReadFromJsonAsync<EventDto>();
        }
    }
}
