using AiCalendar.WebApi.DTOs.Users;

namespace AiCalendar.WebApi.DTOs.Event
{
    public class EventDto
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string CreatorId { get; set; } = string.Empty;

        public bool IsCancelled { get; set; }

        public IEnumerable<UserDto> Participants { get; set; } = new HashSet<UserDto>();
    }
}
