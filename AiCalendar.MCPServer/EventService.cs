using System.Net.Http;
using AiCalendar.WebApi.DTOs.Event;

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

            var eventDto = default(EventDto);
            if (response.IsSuccessStatusCode)
            {
                eventDto = await response.Content.ReadFromJsonAsync<EventDto>();
            }
            
            return eventDto;
        }
    }
}
