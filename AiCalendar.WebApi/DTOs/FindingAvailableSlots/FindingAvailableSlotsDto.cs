using AiCalendar.WebApi.Constants;
using System.ComponentModel.DataAnnotations;

namespace AiCalendar.WebApi.DTOs.FindingAvailableSlots
{
    public class FindingAvailableSlotsDto : IValidatableObject
    {
        public DateTime SearchStartDateTime { get; set; }

        public DateTime SearchEndDateTime { get; set; }

        public int SlotDurationInMinutes { get; set; }

        public int NumberOfSlotsToFind { get; set; } = 1;

        public ICollection<string> ParticipantsIds { get; set; } = new HashSet<string>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SearchStartDateTime >= SearchEndDateTime)
            {
                yield return new ValidationResult(
                    EventConstants.StartTimeGreaterThanOrTheSameAsEndTimeErrorMessage,
                    new[] { nameof(SearchEndDateTime), nameof(SearchEndDateTime) });
            }

            if (SlotDurationInMinutes <= 0)
            {
                yield return new ValidationResult(
                    "Slot duration must be greater than zero.",
                    new[] { nameof(SlotDurationInMinutes) });
            }

            if (NumberOfSlotsToFind <= 0)
            {
                yield return new ValidationResult(
                    "Number of slots to find must be greater than zero.",
                    new[] { nameof(NumberOfSlotsToFind) });
            }
        }
    }
}
