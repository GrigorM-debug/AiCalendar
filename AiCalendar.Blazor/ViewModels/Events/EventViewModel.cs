using AiCalendar.Blazor.ViewModels.Users;

namespace AiCalendar.Blazor.ViewModels.Events
{
    public class EventViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string CreatorId { get; set; } = string.Empty;

        public bool IsCancelled { get; set; }

        public IEnumerable<UserViewModel> Participants { get; set; } = new HashSet<UserViewModel>();
    }
}
