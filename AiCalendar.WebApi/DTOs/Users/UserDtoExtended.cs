using AiCalendar.WebApi.DTOs.Event;

namespace AiCalendar.WebApi.DTOs.Users
{
    public class UserDtoExtended
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public IEnumerable<EventDto> CreatedEvents { get; set; } = new HashSet<EventDto>();

        public IEnumerable<EventDto> ParticipatingEvents { get; set; } = new HashSet<EventDto>();
    }
}
