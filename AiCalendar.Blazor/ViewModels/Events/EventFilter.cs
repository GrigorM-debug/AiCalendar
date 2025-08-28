namespace AiCalendar.Blazor.ViewModels.Events
{
    public class EventFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsCancelled { get; set; }
    }
}
