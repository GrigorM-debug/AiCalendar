namespace AiCalendar.WebApi.DTOs.Event
{
    public class EventFilterCriteriaDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsCancelled { get; set; }
    }
}
