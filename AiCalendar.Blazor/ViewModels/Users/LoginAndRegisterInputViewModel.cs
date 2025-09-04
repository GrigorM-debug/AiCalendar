using System.ComponentModel.DataAnnotations;
using AiCalendar.Blazor.Constants;

namespace AiCalendar.Blazor.ViewModels.Users
{
    public class LoginAndRegisterInputViewModel : IValidatableObject
    {
        /// <summary>
        /// /// Determines whether the username meets conditions.
        /// /// Username conditions:
        /// /// Must be 1 to 24 character in length
        /// /// Must start with letter a-zA-Z
        /// /// May contain letters, numbers or '.','-' or '_'
        /// /// Must not end in '.','-','._' or '-_' 
        /// </summary>
        [Required(ErrorMessage = UserConstants.UserNameRequiredMessage)]
        [StringLength(UserConstants.UserNameMaxLength, ErrorMessage = UserConstants.UserNameLengthErrorMessage, MinimumLength = UserConstants.UserNameMinLength)]
        [DataType(DataType.Text, ErrorMessage = UserConstants.InvalidDataTypeErrorMessage)]
        [RegularExpression(UserConstants.UserNamePattern, ErrorMessage = UserConstants.UserNamePatternErrorMessage)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = UserConstants.EmailRequiredMessage)]
        [EmailAddress(ErrorMessage = UserConstants.EmailFormatErrorMessage)]
        [DataType(DataType.EmailAddress, ErrorMessage = UserConstants.InvalidDataTypeErrorMessage)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = UserConstants.PasswordRequiredMessage)]
        [StringLength(UserConstants.PasswordMaxLength, ErrorMessage = UserConstants.PasswordLengthErrorMessage, MinimumLength = UserConstants.PasswordMinLength)]
        [DataType(DataType.Password, ErrorMessage = UserConstants.InvalidDataTypeErrorMessage)]
        [RegularExpression(UserConstants.PasswordPattern, ErrorMessage = UserConstants.PasswordPatternErrorMessage)]
        public string Password { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password.ToLower().Contains("password"))
            {
                yield return new ValidationResult(UserConstants.PasswordContainsPasswordErrorMessage, new[] { nameof(Password) });
            }

            if (UserName.EndsWith('.') || UserName.EndsWith('_') || UserName.EndsWith('-'))
            {
                yield return new ValidationResult(UserConstants.UserNameInvalidEndingErrorMessage,
                    new[] { nameof(UserName) });
            }
        }
    }
}
