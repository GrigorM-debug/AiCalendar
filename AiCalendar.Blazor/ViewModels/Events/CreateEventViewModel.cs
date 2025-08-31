using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using AiCalendar.Blazor.Constants;

namespace AiCalendar.Blazor.ViewModels.Events
{
    public class CreateEventViewModel : IValidatableObject
    {
        [Required(ErrorMessage = EventConstants.TitleRequiredMessage)]
        [StringLength(EventConstants.TitleMaxLength, ErrorMessage = EventConstants.TitleLengthErrorMessage,
            MinimumLength = EventConstants.TitleMinLength)]
        public string Title { get; set; } = string.Empty;

        [StringLength(EventConstants.DescriptionMaxLength, ErrorMessage = EventConstants.DescriptionLengthErrorMessage,
            MinimumLength = EventConstants.DescriptionMinLength)]
        public string? Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime, ErrorMessage = EventConstants.DateTimeFormatErrorMessage)]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.DateTime, ErrorMessage = EventConstants.DateTimeFormatErrorMessage)]
        public DateTime EndTime { get; set; } = DateTime.UtcNow.AddHours(2);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime >= EndTime)
            {
                yield return new ValidationResult(EventConstants.StartTimeGreaterThanOrTheSameAsEndTimeErrorMessage,
                    new[] { nameof(StartTime), nameof(EndTime) });
            }
        }
    }
}
