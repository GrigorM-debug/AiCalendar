using System.ComponentModel;
using System.Text.Json;
using AiCalendar.WebApi.Data.Repository;
using ModelContextProtocol.Server;

namespace AiCalendar.MCPServer
{
    [McpServerToolType]
    public class EventParticipantsTools
    {
        [McpServerTool, Description("Get user participation events")]
        public static async Task<string> GetEventParticipantsAsync(
            EventParticipantsService eventParticipantsService,
            [Description("The id if the event")] string eventId,
            [Description("The user JWT token")] string jwtToken
        )
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return "EventId can not be null or empty!";
            }

            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can not be null or empty!";
            }

            try
            {
                var response = await eventParticipantsService.GetEventParticipantsAsync(eventId, jwtToken);

                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return $"Error retrieving event participants: {ex.Message}";
            }
        }

        [McpServerTool, Description("Add event participant")]
        public static async Task<string> AddEventParticipantAsync(
            EventParticipantsService eventParticipantsService,
            [Description("The user JWT token")] string jwtToken,
            [Description("The id of the event")] string eventId,
            [Description("The id of the participant to add")]
            string participantId)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can not be null or empty!";
            }

            if (string.IsNullOrEmpty(eventId))
            {
                return "EventId can not be null or empty!";
            }

            if (string.IsNullOrEmpty(participantId))
            {
                return "ParticipantId can not be null or empty!";
            }

            try
            {
                var response = await eventParticipantsService.AddEventParticipant(jwtToken, eventId, participantId);

                return response;
            }
            catch (Exception ex)
            {
                return $"Error adding event participant: {ex.Message}";
            }
        }

        [McpServerTool, Description("Remove event participant")]
        public async Task<string> RemoveEventParticipantAsync(
            EventParticipantsService eventParticipantsService,
            [Description("The user JWT token")] string jwtToken,
            [Description("The id of the event")] string eventId,
            [Description("The id of the participant to remove")]
            string participantId)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                return "JWT token can not be null or empty!";
            }

            if (string.IsNullOrEmpty(eventId))
            {
                return "EventId can not be null or empty!";
            }

            if (string.IsNullOrEmpty(participantId))
            {
                return "ParticipantId can not be null or empty!";
            }

            try
            {
                var response = await eventParticipantsService.RemoveEventParticipant(jwtToken, eventId, participantId);

                return response;
            }
            catch (Exception ex)
            {
                return $"Error removing event participant: {ex.Message}";
            }
        }
    }
}
