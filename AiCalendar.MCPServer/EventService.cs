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

            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"Event with ID {eventId} not found.");
            }

            if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new ArgumentException("Invalid event ID format.");
            }

            if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new Exception("An error occurred while processing your request.");
            }

            var eventDto = default(EventDto);
            if (response.IsSuccessStatusCode)
            {
                eventDto = await response.Content.ReadFromJsonAsync<EventDto>();
            }
            
            return eventDto;
        }
    }
}
