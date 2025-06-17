using AiCalendar.WebApi.DTOs.FindingAvailableSlots;

namespace AiCalendar.WebApi.Services.FindingAvailableSlots
{
    public interface IFindingAvailableSlotsService
    {
        /// <summary>
        /// Asynchronously finds available slots based on the specified search criteria.
        /// </summary>
        /// <param name="request">The request object containing search parameters for available slots.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains collection of <see cref="AvailableSlotsDto"/> with the available slots data.</returns>
        Task<List<AvailableSlotsDto>> FindAvailableSlotsAsync(FindingAvailableSlotsDto request);
    }
}
