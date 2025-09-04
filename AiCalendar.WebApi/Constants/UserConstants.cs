namespace AiCalendar.WebApi.Constants
{
    public static class UserConstants
    {
        //UserName
        public const string UserNameRequiredMessage = "User name is required.";
        public const int UserNameMinLength = 3;
        public const int UserNameMaxLength = 30;
        public const string UserNameLengthErrorMessage = "User name must be between {2} and {1} characters long.";

        public const string UserNamePattern = @"^[a-zA-Z][a-zA-Z0-9\._\-]{3,30}?[a-zA-Z0-9]{0,2}$";
        public const string UserNamePatternErrorMessage = "User name must start with a letter, can contain letters, numbers, '.', '-', or '_'";

        public const string UserNameInvalidEndingErrorMessage = "Username can not end with ., -, _";

        //Email
        public const string EmailRequiredMessage = "Email is required.";
        public const string EmailFormatErrorMessage = "Email format is invalid.";

        //Password 
        public const string PasswordRequiredMessage = "Password is required.";
        public const int PasswordMinLength = 5;
        public const int PasswordMaxLength = 50;
        public const string PasswordLengthErrorMessage = "Password must be between {2} and {1} characters long.";

        // Password pattern: at least one uppercase letter, one lowercase letter, one digit, and one special character
        public const string PasswordPattern = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{5,50}$";
        public const string PasswordPatternErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";
        
        public const string PasswordContainsPasswordErrorMessage = "Password must not contain the word 'password'.";

        public const string InvalidDataTypeErrorMessage = "The data type of the field is invalid.";
    }
}
