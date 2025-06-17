namespace AiCalendar.WebApi.Constants
{
    public static class UserConstants
    {
        //UserName
        public const string UserNameRequiredMessage = "User name is required.";
        public const int UserNameMinLength = 3;
        public const int UserNameMaxLength = 30;
        public const string UserNameLengthErrorMessage = "User name must be between {2} and {1} characters long.";

        //Email
        public const string EmailRequiredMessage = "Email is required.";
        public const string EmailFormatErrorMessage = "Email format is invalid.";

        //Password 
        public const string PasswordRequiredMessage = "Password is required.";
        public const int PasswordMinLength = 5;
        public const int PasswordMaxLength = 50;
        public const string PasswordLengthErrorMessage = "Password must be between {2} and {1} characters long.";
    }
}
