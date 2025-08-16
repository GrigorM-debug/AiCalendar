using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.Extensions;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events.Interfaces;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCalendar.WebApi.Controllers
{
    /// <summary>
    /// Controller for handling events logic
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;

        public EventController(ILogger<EventController> logger, IEventService eventService, IUserService userService)
        {
            _logger = logger;
            _eventService = eventService;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves an event by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the event as a string.</param>
        /// <returns>Returns the event data if found.</returns>
        /// <response code="200">Returns the event data.</response>
        /// <response code="400">Invalid event ID format.</response>
        /// <response code="404">Event not found.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid eventId))
            {
                _logger.LogWarning("Invalid event ID format: {EventId}", id);
                return BadRequest("Invalid user ID format.");
            }

            bool isEventExists = await _eventService.EventExistsByIdAsync(eventId);

            if (!isEventExists)
            {
                _logger.LogWarning("Event not found for ID: {EventId}", eventId);
                return NotFound("Event not found.");
            }

            EventDto? eventDto = await _eventService.GetEventByIdAsync(eventId);

            return Ok(eventDto);
        }

        /// <summary>
        /// Creates a new event for the authenticated user.
        /// </summary>
        /// <param name="createEventDto">The details of the event to create.</param>
        /// <returns>Returns the created event.</returns>
        /// <response code="201">Event created successfully.</response>
        /// <response code="400">Invalid user ID or event data.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="409">User already has an event scheduled for this time period.</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateEventAsync([FromBody] CreateEventDto createEventDto)
        {
            string? userIdString = User.GetUserId();

            if (User?.Identity == null || User?.Identity?.IsAuthenticated == false || userIdString == null)
            {
                _logger.LogWarning("Unauthorized attempt to create an event without authentication.");
                return Unauthorized("You must be logged in to create an event.");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", userIdString);
                return BadRequest("Invalid user ID.");
            }

            bool isUserExists = await _userService.UserExistsByIdAsync(userId);
            if (!isUserExists)
            {
                _logger.LogWarning("User not found for ID: {UserId}", userId);
                return NotFound("User not found.");
            }

            bool hasOverlappingEvents = await _eventService.HasOverlappingEvents(userId, createEventDto.StartTime, createEventDto.EndTime);

            if (hasOverlappingEvents)
            {
                _logger.LogWarning("User {UserId} already has an event scheduled for the time period: {StartTime} - {EndTime}", userId, createEventDto.StartTime, createEventDto.EndTime);
                return Conflict("You already have an event scheduled for this time period");
            }

            EventDto createdEvent = await _eventService.CreateEventAsync(createEventDto, userId);

            return StatusCode(201, createdEvent);
        }

        /// <summary>
        /// Deletes an event by its unique identifier for the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the event as a string.</param>
        /// <returns>No content if the event is deleted successfully.</returns>
        /// <response code="204">Event deleted successfully.</response>
        /// <response code="400">Invalid user ID or event ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not the creator of the event.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            string? userIdString = User.GetUserId();

            if (User?.Identity == null || User?.Identity?.IsAuthenticated == false || userIdString == null)
            {
                _logger.LogWarning("Unauthorized attempt to delete an event without authentication.");
                return Unauthorized("You must be logged in to delete an event.");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", userIdString);
                return BadRequest("Invalid user ID.");
            }

            if (!Guid.TryParse(id, out Guid eventId))
            {
                _logger.LogWarning("Invalid event ID format: {EventId}", id);
                return BadRequest("Invalid event ID format.");
            }

            bool isUserExists = await _userService.UserExistsByIdAsync(userId);
            if (!isUserExists)
            {
                _logger.LogWarning("User not found for ID: {UserId}", userId);
                return NotFound("User not found.");
            }

            bool isEventExists = await _eventService.EventExistsByIdAsync(eventId);

            if (!isEventExists)
            {
                _logger.LogWarning("Event not found for ID: {EventId}", eventId);
                return NotFound("Event not found.");
            }

            bool isUserEventCreator = await _eventService.IsUserEventCreator(eventId, userId);

            if (!isUserEventCreator)
            {
                _logger.LogWarning("User {UserId} is not the creator of the event with ID: {EventId}", userId, eventId);
                return Forbid("You are not the creator of this event.");
            }

            await _eventService.DeleteEventAsync(eventId);

            return NoContent();
        }

        /// <summary>
        /// Updates an existing event for the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the event as a string.</param>
        /// <param name="updateEventDto">The updated event details.</param>
        /// <returns>Returns the updated event.</returns>
        /// <response code="200">Event updated successfully.</response>
        /// <response code="400">Invalid user ID or event ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not the creator of the event.</response>
        /// <response code="404">Event not found.</response>
        /// <response code="409">User already has an event scheduled for this time period.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateEventAsync(string id, [FromBody] UpdateEventDto updateEventDto)
        {
            string? userIdString = User.GetUserId();

            if (User?.Identity == null || User?.Identity?.IsAuthenticated == false || userIdString == null)
            {
                _logger.LogWarning("Unauthorized attempt to update an event without authentication.");
                return Unauthorized("You must be logged in to update an event.");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", userIdString);
                return BadRequest("Invalid user ID.");
            }

            if (!Guid.TryParse(id, out Guid eventId))
            {
                _logger.LogWarning("Invalid event ID format: {EventId}", id);
                return BadRequest("Invalid event ID format.");
            }

            bool isUserExists = await _userService.UserExistsByIdAsync(userId);
            if (!isUserExists)
            {
                _logger.LogWarning("User not found for ID: {UserId}", userId);
                return NotFound("User not found.");
            }

            bool isEventExists = await _eventService.EventExistsByIdAsync(eventId);
            if (!isEventExists)
            {
                _logger.LogWarning("Event not found for ID: {EventId}", eventId);
                return NotFound("Event not found.");
            }

            bool isUserEventCreator = await _eventService.IsUserEventCreator(eventId, userId);
            if (!isUserEventCreator)
            {
                _logger.LogWarning("User {UserId} is not the creator of the event with ID: {EventId}", userId, eventId);
                return Forbid("You are not the creator of this event.");
            }

            bool isEventWithDataAlreadyExists = await _eventService
                .CheckIfEventExistsByTitleAndDescription(
                    updateEventDto.Title, 
                    updateEventDto.Description,
                    userId);

            if (isEventWithDataAlreadyExists)
            {
                _logger.LogWarning("An event with the same title and description already exists.");
                return Conflict("An event with the same title and description already exists.");
            }

            if (updateEventDto.StartTime.HasValue && updateEventDto.EndTime.HasValue)
            {
                bool hasOverlappingEventsExcludingTheCurrentOne =
                    await _eventService.HasOverlappingEventsExcludingTheCurrentEvent(userId, updateEventDto.StartTime.Value,
                        updateEventDto.EndTime.Value, eventId);

                if (hasOverlappingEventsExcludingTheCurrentOne)
                {
                    _logger.LogWarning("User {UserId} already has an event scheduled for the time period: {StartTime} - {EndTime}", userId, updateEventDto.StartTime, updateEventDto.EndTime);
                    return Conflict("You already have an event scheduled for this time period");
                }
            }

            EventDto updatedEvent = await _eventService.UpdateEvent(eventId, updateEventDto, userId);

            return Ok(updatedEvent);
        }

        /// <summary>
        /// Cancels an event by its unique identifier for the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the event as a string.</param>
        /// <returns>Returns the cancelled event.</returns>
        /// <response code="200">Event cancelled successfully.</response>
        /// <response code="400">Invalid user ID or event ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not the creator of the event.</response>
        /// <response code="404">Event not found.</response>
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelEventAsync(string id)
        {
            string? userIdString = User.GetUserId();

            if (User?.Identity == null || User?.Identity?.IsAuthenticated == false || userIdString == null)
            {
                _logger.LogWarning("Unauthorized attempt to cancel an event without authentication.");
                return Unauthorized("You must be logged in to cancel an event.");
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                _logger.LogWarning("Invalid user ID format: {UserId}", userIdString);
                return BadRequest("Invalid user ID.");
            }

            if (!Guid.TryParse(id, out Guid eventId))
            {
                _logger.LogWarning("Invalid event ID format: {EventId}", id);
                return BadRequest("Invalid event ID format.");
            }

            bool isUserExists = await _userService.UserExistsByIdAsync(userId);
            if (!isUserExists) 
            {
                _logger.LogWarning("User not found for ID: {UserId}", userId);
                return NotFound("User not found.");
            }

            bool isEventExists = await _eventService.EventExistsByIdAsync(eventId);
            if (!isEventExists)
            {
                _logger.LogWarning("Event not found for ID: {EventId}", eventId);
                return NotFound("Event not found.");
            }

            bool isUserEventCreator = await _eventService.IsUserEventCreator(eventId, userId);
            if (!isUserEventCreator)
            {
                _logger.LogWarning("User {UserId} is not the creator of the event with ID: {EventId}", userId, eventId);
                return Forbid("You are not the creator of this event.");
            }

            bool isEventCancelled = await _eventService.CheckIfEventIsAlreadyCancelled(eventId);
            if (isEventCancelled)
            {
                _logger.LogWarning("Event with ID: {EventId} is already cancelled.", eventId);
                return BadRequest("Event is already cancelled.");
            }

            EventDto cancelledEvent = await _eventService.CancelEventAsync(eventId, userId);

            return Ok(cancelledEvent);
        }

        /// <summary>
        /// Gets events with optional filtering
        /// </summary>
        /// <param name="filter">Optional filter criteria for events</param>
        /// <returns>Returns collection of filtered events</returns>
        /// <response code="200">Returns the filtered list of events</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents([FromQuery] EventFilterCriteriaDto? filter)
        {
            IEnumerable<EventDto> events = await _eventService.GetEventsAsync(filter);

            return Ok(events);
        }

    }
}
