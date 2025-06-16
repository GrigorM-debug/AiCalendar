using AiCalendar.WebApi.Constants;
using System.ComponentModel.DataAnnotations;

namespace AiCalendar.WebApi.DTOs.Event
{
    public class UpdateEventDto : IValidatableObject
    {
        [Required(ErrorMessage = EventConstants.TitleRequiredMessage)]
        [StringLength(EventConstants.TitleMaxLength, ErrorMessage = EventConstants.TitleLengthErrorMessage, MinimumLength = EventConstants.TitleMinLength)]
        public string Title { get; set; } = string.Empty;

        [StringLength(EventConstants.DescriptionMaxLength, ErrorMessage = EventConstants.DescriptionLengthErrorMessage, MinimumLength = EventConstants.DescriptionMinLength)]
        public string? Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime, ErrorMessage = EventConstants.DateTimeFormatErrorMessage)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime, ErrorMessage = EventConstants.DateTimeFormatErrorMessage)]
        public DateTime EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime >= EndTime)
            {
                yield return new ValidationResult(
                    EventConstants.StartTimeGreaterThanOrTheSameAsEndTimeErrorMessage,
                    new[] { nameof(StartTime), nameof(EndTime) });
            }
        }
    }
}
