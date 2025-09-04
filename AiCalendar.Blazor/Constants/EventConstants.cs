namespace AiCalendar.Blazor.Constants
{
    public static class EventConstants
    {
        //Title
        public const string TitleRequiredMessage = "Title is required.";
        public const int TitleMinLength = 5;
        public const int TitleMaxLength = 100;
        public const string TitleLengthErrorMessage = "Title must between {2} and {1} characters long.";

        //Description
        public const int DescriptionMinLength = 30;
        public const int DescriptionMaxLength = 2000;
        public const string DescriptionLengthErrorMessage = "Description must between {2} and {1} characters long.";

        public const string DateTimeFormatErrorMessage = "Invalid DateTimeFormat";

        public const string StartTimeGreaterThanOrTheSameAsEndTimeErrorMessage = "Start time must be less than end time and not equal to end time.";

        public const string InvalidDataTypeErrorMessage = "The data type of the field is invalid.";
    }
}
