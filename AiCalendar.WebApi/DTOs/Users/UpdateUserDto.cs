using AiCalendar.WebApi.Constants;
using System.ComponentModel.DataAnnotations;

namespace AiCalendar.WebApi.DTOs.Users
{
    public class UpdateUserDto : IValidatableObject
    {
        /// <summary>
        /// /// Determines whether the username meets conditions.
        /// /// Username conditions:
        /// /// Must be 1 to 24 character in length
        /// /// Must start with letter a-zA-Z
        /// /// May contain letters, numbers or '.','-' or '_'
        /// /// Must not end in '.','-','._' or '-_' 
        /// </summary>
        public string? UserName { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public string? OldPassword { get; set; } = string.Empty;

        public string? NewPassword { get; set; } = string.Empty;


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Check the username length if it's provided
            if (!string.IsNullOrEmpty(UserName))
            {
                if (UserName.Length < UserConstants.UserNameMinLength && UserName.Length > UserConstants.UserNameMaxLength)
                {
                    yield return new ValidationResult(UserConstants.UserNameLengthErrorMessage, new[] { nameof(UserName) });
                }

                var userNameType = new DataTypeAttribute(DataType.Text);
                if (!userNameType.IsValid(UserName))
                {
                    yield return new ValidationResult(UserConstants.InvalidDataTypeErrorMessage, new[] { nameof(UserName) });
                }

                var usernamePattern = new System.Text.RegularExpressions.Regex(UserConstants.UserNamePattern);

                if (!usernamePattern.IsMatch(UserName))
                {
                    yield return new ValidationResult(UserConstants.UserNamePatternErrorMessage, new[] { nameof(UserName) });
                }

                if (UserName.EndsWith('.') || UserName.EndsWith('_') || UserName.EndsWith('-'))
                {
                    yield return new ValidationResult(UserConstants.UserNameInvalidEndingErrorMessage,
                        new[] { nameof(UserName) });
                }
            }

            // Check the email format if it's provided
            if (!string.IsNullOrEmpty(Email))
            {
                EmailAddressAttribute emailAttribute = new EmailAddressAttribute();
                if (!emailAttribute.IsValid(Email))
                {
                    yield return new ValidationResult(UserConstants.EmailFormatErrorMessage, new[] { nameof(Email) });
                }

                var emailType = new DataTypeAttribute(DataType.EmailAddress);
                if (!emailType.IsValid(Email))
                {
                    yield return new ValidationResult(UserConstants.InvalidDataTypeErrorMessage, new[] { nameof(Email) });
                }
            }

            if (!string.IsNullOrEmpty(OldPassword) && !string.IsNullOrEmpty(NewPassword))
            {
                if (OldPassword.Length < UserConstants.PasswordMinLength &&
                    OldPassword.Length > UserConstants.PasswordMaxLength)
                {
                    yield return new ValidationResult(UserConstants.PasswordLengthErrorMessage,
                        new[] { nameof(OldPassword) });
                }

                if (NewPassword.Length < UserConstants.PasswordMinLength &&
                    NewPassword.Length > UserConstants.PasswordMaxLength)
                {
                    yield return new ValidationResult(UserConstants.PasswordLengthErrorMessage,
                        new[] { nameof(NewPassword) });
                }

                //Check if passwords are valid data type
                var passwordType = new DataTypeAttribute(DataType.Password);
                if (!passwordType.IsValid(OldPassword))
                {
                    yield return new ValidationResult(UserConstants.InvalidDataTypeErrorMessage,
                        new[] { nameof(OldPassword) });
                }

                if (!passwordType.IsValid(NewPassword))
                {
                    yield return new ValidationResult(UserConstants.InvalidDataTypeErrorMessage,
                        new[] { nameof(NewPassword) });
                }

                if (NewPassword == OldPassword)
                {
                    yield return new ValidationResult("New Password can't be the same as the old password",
                        new[] { nameof(NewPassword) });
                }

                if (NewPassword.ToLower().Contains("password"))
                {
                    yield return new ValidationResult(UserConstants.PasswordContainsPasswordErrorMessage,
                        new[] { nameof(NewPassword) });
                }

                // Check the new password pattern
                var passwordPattern = new System.Text.RegularExpressions.Regex(UserConstants.PasswordPattern);
                if (!passwordPattern.IsMatch(NewPassword))
                {
                    yield return new ValidationResult(UserConstants.PasswordPatternErrorMessage,
                        new[] { nameof(NewPassword) });
                }
            }
        }
    }
}
