using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AiCalendar.WebApi.Constants;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Models
{
    public class Event
    {
        [Key]
        [Required]
        [Comment("The id of the event")] 
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = EventConstants.TitleRequiredMessage)]
        [StringLength(EventConstants.TitleMaxLength, ErrorMessage = EventConstants.TitleLengthErrorMessage, MinimumLength = EventConstants.TitleMinLength)]
        [Comment("The title of the event")]
        public string Title { get; set; } = string.Empty;

        [StringLength(EventConstants.DescriptionMaxLength, ErrorMessage = EventConstants.DescriptionLengthErrorMessage, MinimumLength = EventConstants.DescriptionMinLength)]
        [Comment("The description of the event")]
        public string? Description { get; set; } = string.Empty;

        [Required]
        [Comment("The start time of the event")]
        public DateTime StartTime { get; set; }

        [Required]
        [Comment("The end time of the event")]
        public DateTime EndTime { get; set; }

        [Required]
        [Comment("The id of the user who created the event")]
        public Guid CreatorId { get; set; }

        [Required]
        [ForeignKey(nameof(CreatorId))]
        [Comment("User navigation property")]
        public User Creator { get; set; } = null!;

        [Required]
        [Comment("If event is cancelled or no")]
        public bool IsCancelled { get; set; }

        [Comment("Collection of event participants")]
        public ICollection<Participant> Participants { get; set; } = new HashSet<Participant>();
    }
}
