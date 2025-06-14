namespace AiCalendar.WebApi.Constants
{
    public static class EventConstants
    {
        //Title
        public const string TitleRequiredMessage = "Title is required.";
        public const int TitleMinLength = 5;
        public const int TitleMaxLength = 100;
        public const string TitleLengthErrorMessage = "Title must between {1} and {2} characters long.";

        //Description
        public const int DescriptionMinLength = 30;
        public const int DescriptionMaxLength = 2000;
        public const string DescriptionLengthErrorMessage = "Description must between {1} and {2} characters long.";


    }
}
