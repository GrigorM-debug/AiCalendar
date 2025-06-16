using AiCalendar.WebApi.DTOs.Users;
using AiCalendar.WebApi.Services.EventParticipants.Interfaces;
using AiCalendar.WebApi.Services.Events.Interfaces;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AiCalendar.WebApi.Controllers
{
    /// <summary>
    /// Controller for handling event participants operations such as retrieving, adding, and removing participants from events.
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    public class EventParticipantsController : ControllerBase
    {
        private readonly ILogger<EventParticipantsController> _logger;
        private readonly IEventService _eventService;
        private readonly IEventParticipantsService _eventParticipantsService;
        private readonly IUserService _userService;

        public EventParticipantsController(
            ILogger<EventParticipantsController> logger,
            IEventService eventService,
            IEventParticipantsService eventParticipantsService, IUserService userService)
        {
            _logger = logger;
            _eventService = eventService;
            _eventParticipantsService = eventParticipantsService;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all participants for a specified event if the user is authorized.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event as a string.</param>
        /// <returns>Returns the collection of event participants.</returns>
        /// <response code="200">Returns the list of participants for the event.</response>
        /// <response code="400">Invalid user ID or event ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized to view participants for this event.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("/events/{eventId}/participants")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventParticipantsAsync(string eventId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("You must be logged in to create an event.");
            }

            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }

            //Check if eventId is a valid Guid
            if (!Guid.TryParse(eventId, out var parsedEventId))
            {
                _logger.LogError("Invalid eventId format: {EventId}", eventId);
                return BadRequest("Invalid eventId format.");
            }

            //Check if event exists
            bool eventExists = await _eventService.EventExistsByIdAsync(parsedEventId);
            if (eventExists)
            {
                _logger.LogError("Event not found: {EventId}", parsedEventId);
                return NotFound($"Event with ID {parsedEventId} not found.");
            }

            //Check if user is event creator or user is event participant
            bool isUserEventCreator = await _eventService.IsUserEventCreator(parsedEventId, userId);

            bool isUserEventParticipant = await _eventParticipantsService.IsUserEventParticipant(parsedEventId, userId);
            if (!isUserEventCreator && !isUserEventParticipant)
            {
                _logger.LogError("User {UserId} is not authorized to view participants for event {EventId}", userId, parsedEventId);
                return Forbid("You are not authorized to view participants for this event.");
            }

            //Get event participants
            IEnumerable<UserDto> participants = await _eventParticipantsService.GetParticipantsByEventIdAsync(parsedEventId);


            return Ok(participants); 
        }

        /// <summary>
        /// Adds a user as a participant to the specified event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event as a string.</param>
        /// <param name="participantId">The unique identifier of the user to add as a participant (as a string).</param>
        /// <returns>No content if the participant was added successfully.</returns>
        /// <response code="204">Participant added successfully.</response>
        /// <response code="400">Invalid user ID or event ID format, or user is already a participant.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized to add this participant.</response>
        /// <response code="404">Event not found.</response>
        [HttpPost("/events/{eventId}/participants/{participantId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddEventParticipant(string eventId, string participantId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("You must be logged in to add a participant.");
            }

            if (!Guid.TryParse(eventId, out Guid parsedEventId))
            {
                _logger.LogError("Invalid eventId format: {EventId}", eventId);
                return BadRequest("Invalid eventId format.");
            }

            if (!Guid.TryParse(participantId, out Guid parsedParticipantId))
            {
                _logger.LogError("Invalid participantId format: {ParticipantId}", participantId);
                return BadRequest("Invalid participantId format.");
            }

            // Check if event exists
            bool eventExists = await _eventService.EventExistsByIdAsync(parsedEventId);
            if (!eventExists)
            {
                _logger.LogError("Event not found: {EventId}", parsedEventId);
                return NotFound($"Event with ID {parsedEventId} not found.");
            }

            //Check if the current user is authorized to add participants (e.g., is event creator)
            var currentUserId = Guid.Parse(User.FindFirst("UserId")!.Value);
            bool isUserEventCreator = await _eventService.IsUserEventCreator(parsedEventId, currentUserId);
            if (!isUserEventCreator)
            {
                return Forbid("You are not authorized to add participants to this event.");
            }

            //Check if the participant exists
            bool participantExists = await _userService.UserExistsByIdAsync(parsedParticipantId);
            if (!participantExists)
            {
                _logger.LogError("Participant not found: {ParticipantId}", parsedParticipantId);
                return NotFound($"Participant with ID {parsedParticipantId} not found.");
            }

            // Check if the participant is already a participant in the event
            bool isParticipantAlready = await _eventParticipantsService.IsUserEventParticipant(parsedParticipantId, parsedEventId);
            if (isParticipantAlready)
            {
                _logger.LogError("User {ParticipantId} is already a participant in event {EventId}", parsedParticipantId, parsedEventId);
                return BadRequest($"User with ID {parsedParticipantId} is already a participant in event {parsedEventId}.");
            }

            // Add participant to the event
            await _eventParticipantsService.AddParticipantAsync(parsedParticipantId, parsedEventId);

            _logger.LogInformation("User {ParticipantId} added as a participant to event {EventId}", parsedParticipantId, parsedEventId);

            return NoContent(); 
        }

        /// <summary>
        /// Removes a user as a participant from the specified event.
        /// </summary>
        /// <param name="eventId">The unique identifier of the event as a string.</param>
        /// <param name="participantId">The unique identifier of the user to remove as a participant (as a string).</param>
        /// <returns>No content if the participant was removed successfully.</returns>
        /// <response code="204">Participant removed successfully.</response>
        /// <response code="400">Invalid user ID or event ID format, or participant not found.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized to remove this participant.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("/events/{eventId}/participants/{participantId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveParticipantAsync(string eventId, string participantId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("You must be logged in to remove a participant.");
            }

            if (!Guid.TryParse(eventId, out Guid parsedEventId))
            {
                _logger.LogError("Invalid eventId format: {EventId}", eventId);
                return BadRequest("Invalid eventId format.");
            }

            if (!Guid.TryParse(participantId, out Guid parsedParticipantId))
            {
                _logger.LogError("Invalid participantId format: {ParticipantId}", participantId);
                return BadRequest("Invalid participantId format.");
            }

            // Check if event exists
            bool eventExists = await _eventService.EventExistsByIdAsync(parsedEventId);
            if (!eventExists)
            {
                _logger.LogError("Event not found: {EventId}", parsedEventId);
                return NotFound($"Event with ID {parsedEventId} not found.");
            }

            // Only event creator can remove participants
            var currentUserId = Guid.Parse(User.FindFirst("UserId")!.Value);
            bool isUserEventCreator = await _eventService.IsUserEventCreator(parsedEventId, currentUserId);
            if (!isUserEventCreator)
            {
                return Forbid("You are not authorized to remove participants from this event.");
            }

            // Check if participant exists
            bool isParticipant = await _eventParticipantsService.IsUserEventParticipant(parsedParticipantId, parsedEventId);
            if (!isParticipant)
            {
                return BadRequest("User is not a participant in this event.");
            }

            await _eventParticipantsService.RemoveParticipantAsync(parsedParticipantId, parsedEventId);

            return NoContent();
        }
    }
}
