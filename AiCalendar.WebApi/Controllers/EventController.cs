using AiCalendar.WebApi.DTOs.Event;
using AiCalendar.WebApi.Models;
using AiCalendar.WebApi.Services.Events.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCalendar.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;

        public EventController(ILogger<EventController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid eventId))
            {
                return BadRequest("Invalid user ID format.");
            }

            bool isEventExists = await _eventService.EventExistsByIdAsync(eventId);

            if (!isEventExists)
            {
                return NotFound("Event not found.");
            }

            EventDto eventDto = await _eventService.GetEventByIdAsync(eventId);

            return Ok(eventDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEventAsync([FromBody] CreateEventDto createEventDto)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("You must be logged in to create an event.");
            }

            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }

            if (createEventDto == null)
            {
                return BadRequest("Invalid event data.");
            }

            bool hasOverlappingEvents = await _eventService.HasOverlappingEvents(userId, createEventDto.StartTime, createEventDto.EndTime);

            if (hasOverlappingEvents)
            {
                return Conflict("You already have an event scheduled for this time period");
            }

            EventDto createdEvent = await _eventService.CreateEventAsync(createEventDto, userId);

            return StatusCode(201, createdEvent);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("You must be logged in to create an event.");
            }

            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }

            if (!Guid.TryParse(id, out Guid eventId))
            {
                return BadRequest("Invalid user ID format.");
            }

            bool isEventExists = await _eventService.EventExistsByIdAsync(eventId);

            if (!isEventExists)
            {
                return NotFound("Event not found.");
            }

            bool isUserEventCreator = await _eventService.IsUserEventCreator(eventId, userId);

            if (!isUserEventCreator)
            {
                return Forbid("You are not the creator of this event.");
            }

            await _eventService.DeleteEventAsync(eventId);

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEventAsync(string id, [FromBody] UpdateEventDto updateEventDto)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("You must be logged in to update an event.");
            }

            if (!Guid.TryParse(User.FindFirst("UserId")?.Value, out Guid userId))
            {
                return BadRequest("Invalid user ID.");
            }

            if (!Guid.TryParse(id, out Guid eventId))
            {
                return BadRequest("Invalid event ID format.");
            }

            bool isEventExists = await _eventService.EventExistsByIdAsync(eventId);
            if (!isEventExists)
            {
                return NotFound("Event not found.");
            }

            bool isUserEventCreator = await _eventService.IsUserEventCreator(eventId, userId);
            if (!isUserEventCreator)
            {
                return Forbid("You are not the creator of this event.");
            }

            bool hasOverlappingEventsExcludingTheCurrentOne =
                await _eventService.HasOverlappingEventsExcludingTheCurrentEvent(userId, updateEventDto.StartTime,
                    updateEventDto.EndTime, eventId);

            if (hasOverlappingEventsExcludingTheCurrentOne)
            {
                return Conflict("You already have an event scheduled for this time period");
            }

            EventDto updatedEvent = await _eventService.UpdateEvent(eventId, updateEventDto, userId);

            return Ok(updatedEvent);
        }
    }
}
