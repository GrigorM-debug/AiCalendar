using System.ComponentModel.DataAnnotations;
using AiCalendar.Blazor.Constants;

namespace AiCalendar.Blazor.ViewModels.Users
{
    public class UpdateUserViewModel : IValidatableObject
    {
        public string? UserName { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public string? OldPassword { get; set; } = string.Empty;

        public string? NewPassword { get; set; } = string.Empty;


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(UserName))
            {
                if (UserName.Length < UserConstants.UserNameMinLength || UserName.Length > UserConstants.UserNameMaxLength)
                {
                    yield return new ValidationResult(UserConstants.UserNameLengthErrorMessage, new[] { nameof(UserName) });
                }
            }

            if (!string.IsNullOrEmpty(Email))
            {
                EmailAddressAttribute emailAttribute = new EmailAddressAttribute();
                if (!emailAttribute.IsValid(Email))
                {
                    yield return new ValidationResult(UserConstants.EmailFormatErrorMessage, new[] { nameof(Email) });
                }
            }

            if (!string.IsNullOrEmpty(OldPassword) && !string.IsNullOrEmpty(NewPassword))
            {
                if (OldPassword.Length < UserConstants.PasswordMinLength || OldPassword.Length > UserConstants.PasswordMaxLength)
                {
                    yield return new ValidationResult(UserConstants.PasswordLengthErrorMessage, new[] { nameof(OldPassword) });
                }

                if (NewPassword.Length < UserConstants.PasswordMinLength || NewPassword.Length > UserConstants.PasswordMaxLength)
                {
                    yield return new ValidationResult(UserConstants.PasswordLengthErrorMessage, new[] { nameof(NewPassword) });
                }

                if (NewPassword == OldPassword)
                {
                    yield return new ValidationResult("New Password can't be the same as the old password", new[] { nameof(NewPassword) });
                }
            }
        }
    }
}
