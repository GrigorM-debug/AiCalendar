using System.ComponentModel.DataAnnotations;
using AiCalendar.WebApi.Constants;

namespace AiCalendar.WebApi.DTOs.Users
{
    public class LoginAndRegisterInputDto
    {
        [Required(ErrorMessage = UserConstants.UserNameRequiredMessage)]
        [StringLength(UserConstants.UserNameMaxLength, ErrorMessage = UserConstants.UserNameLengthErrorMessage, MinimumLength = UserConstants.UserNameMinLength)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = UserConstants.EmailRequiredMessage)]
        [EmailAddress(ErrorMessage = UserConstants.EmailFormatErrorMessage)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = UserConstants.PasswordRequiredMessage)]
        [StringLength(UserConstants.PasswordMaxLength, ErrorMessage = UserConstants.PasswordLengthErrorMessage, MinimumLength = UserConstants.PasswordMinLength)]
        public string Password { get; set; } = string.Empty;
    }
}
