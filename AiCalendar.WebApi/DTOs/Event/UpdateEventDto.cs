﻿using AiCalendar.WebApi.Constants;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic.CompilerServices;

namespace AiCalendar.WebApi.DTOs.Event
{
    public class UpdateEventDto : IValidatableObject
    {
        public string? Title { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        [DataType(DataType.DateTime, ErrorMessage = EventConstants.DateTimeFormatErrorMessage)]
        public DateTime? StartTime { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = EventConstants.DateTimeFormatErrorMessage)]
        public DateTime? EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                if (Title.Length < EventConstants.TitleMinLength || Title.Length > EventConstants.TitleMaxLength)
                {
                    yield return new ValidationResult(
                        EventConstants.TitleLengthErrorMessage,
                        new[] { nameof(Title) });
                }
            }

            if (!string.IsNullOrEmpty(Description))
            {
                if (Description.Length < EventConstants.DescriptionMinLength ||
                    Description.Length > EventConstants.DescriptionMaxLength)
                {
                    yield return new ValidationResult(
                        EventConstants.DescriptionLengthErrorMessage,
                        new[] { nameof(Description) });
                }
            }

            if (StartTime.HasValue && EndTime.HasValue)
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
}
