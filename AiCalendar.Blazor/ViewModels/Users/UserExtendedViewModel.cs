using AiCalendar.Blazor.ViewModels.Events;

namespace AiCalendar.Blazor.ViewModels.Users
{
    public class UserExtendedViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public IEnumerable<EventViewModel> CreatedEvents { get; set; } = new HashSet<EventViewModel>();

        public IEnumerable<EventViewModel> ParticipatingEvents { get; set; } = new HashSet<EventViewModel>();
    }
}
