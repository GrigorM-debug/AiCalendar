using AiCalendar.WebApi.DTOs.Event;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace AiCalendar.MCPServer
{
    [McpServerToolType]
    public static class EventTools
    {
        [McpServerTool, Description("Get an event by its ID")]
        public static async Task<string> GetEventByIdAsync(EventService eventService, [Description("The id of the event")] string eventId)
        {
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
    }
}
