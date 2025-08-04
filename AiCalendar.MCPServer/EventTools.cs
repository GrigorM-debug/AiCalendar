using AiCalendar.WebApi.DTOs.Event;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AiCalendar.MCPServer
{
    [McpServerToolType]
    public static class EventTools
    {
        [McpServerTool, Description("Get an event by its ID")]
        public static async Task<string> GetEventByIdAsync(EventService eventService, [Description("The id of the event")] string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return "EventId can't be null or empty!";
            }

            try
            {
                var response = await eventService.GetEventByIdAsync(eventId);

                if (response == null)
                {
                    return "Event not found.";
                }

                return JsonSerializer.Serialize(response);
            }
            catch(Exception ex)
            {
                return $"Error retrieving event: {ex.Message}";
            }
        }

        [McpServerTool, Description("Create a new event")]
        public static async Task<string> CreateEventAsync(
            EventService eventService,
            [Description("The JWT token for authentication")] string jwtToken,
            [Description("The event data in JSON format")] string eventJson)
        {
            if (string.IsNullOrEmpty(eventJson))
            {
                return "Event data can't be null or empty!";
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can't be null or empty!";
            }

            try
            {
                var eventDto = JsonSerializer.Deserialize<EventDto>(eventJson);

                if (eventDto == null)
                {
                    return "Invalid event data format.";
                }

                var response = await eventService.CreateEventAsync(eventDto, jwtToken);
                
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error creating event: {ex.Message}";
            }
        }

        [McpServerTool, Description("Delete event")]
        public static async Task<string> DeleteEvent(
            EventService eventService,
            [Description("User JWT token")] string jwtToken,
            [Description("The id of the event")] string eventId)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can not be null or empty!";
            }

            if (string.IsNullOrEmpty(eventId))
            {
                return "EventId can not be null or empty!";
            }

            try
            {
                var response = await eventService.DeleteEvent(eventId, jwtToken);

                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error creating event: {ex.Message}";
            }
        }
    }
}
