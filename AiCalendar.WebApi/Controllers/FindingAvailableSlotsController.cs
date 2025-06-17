using AiCalendar.WebApi.DTOs.FindingAvailableSlots;
using AiCalendar.WebApi.Services.FindingAvailableSlots;
using AiCalendar.WebApi.Services.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AiCalendar.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FindingAvailableSlotsController : ControllerBase
    {
        private readonly ILogger<FindingAvailableSlotsController> _logger;
        private readonly IFindingAvailableSlotsService _findingAvailableSlotsService;
        private readonly IUserService _userService;

        public FindingAvailableSlotsController(
            ILogger<FindingAvailableSlotsController> logger,
            IFindingAvailableSlotsService findingAvailableSlotsService, IUserService userService)
        {
            _logger = logger;
            _findingAvailableSlotsService = findingAvailableSlotsService;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves a list of available time slots based on the provided participants' availability.
        /// </summary>
        /// <param name="findingAvailableSlotsDto">
        /// The criteria for finding available slots, including a list of participant IDs.
        /// </param>
        /// <returns>
        /// A list of available time slots.
        /// </returns>
        /// <response code="200">Returns a list of available time slots.</response>
        /// <response code="400">Invalid input (e.g., empty participant list, invalid ID format, or non-existent participant).</response>
        [HttpGet("/available-slots")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AvailableSlotsDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableSlotsAsync([FromBody] FindingAvailableSlotsDto findingAvailableSlotsDto)
        {
            //Check if participants exist

            if (findingAvailableSlotsDto.ParticipantsIds.Count == 0)
            {
                foreach (var participantId in findingAvailableSlotsDto.ParticipantsIds)
                {
                    //Check if id is valid Guid
                    if (!Guid.TryParse(participantId, out Guid participantIdGuid))
                    {
                        return BadRequest("Invalid user ID format.");
                    }

                    //Check if user exists
                    bool userExists = await _userService.UserExistsByIdAsync(participantIdGuid);

                    if (!userExists)
                    {
                        return BadRequest($"User with ID {participantId} does not exist.");
                    }
                }
            }

            List<AvailableSlotsDto> availableFreeSlots =
                await _findingAvailableSlotsService.FindAvailableSlotsAsync(findingAvailableSlotsDto);

            return Ok(availableFreeSlots);
        }
    }
}
