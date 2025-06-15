using AiCalendar.WebApi.Constants;
using System.ComponentModel.DataAnnotations;

namespace AiCalendar.WebApi.DTOs.Users
{
    public class UpdateUserDto
    {
        [StringLength(UserConstants.UserNameMaxLength, ErrorMessage = UserConstants.UserNameLengthErrorMessage, MinimumLength = UserConstants.UserNameMinLength)]
        public string? UserName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = UserConstants.EmailFormatErrorMessage)]
        public string? Email { get; set; } = string.Empty;

        [StringLength(UserConstants.PasswordMaxLength, ErrorMessage = UserConstants.PasswordLengthErrorMessage, MinimumLength = UserConstants.PasswordMinLength)]
        public string? OldPassword { get; set; } = string.Empty;

        [StringLength(UserConstants.PasswordMaxLength, ErrorMessage = UserConstants.PasswordLengthErrorMessage, MinimumLength = UserConstants.PasswordMinLength)]
        public string? NewPassword { get; set; } = string.Empty;
    }
}
