using System.ComponentModel.DataAnnotations;
using AiCalendar.WebApi.Constants;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Models
{
    public class User
    {
        [Key]
        [Required]
        [Comment("The id of the user")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = UserConstants.UserNameRequiredMessage)]
        [StringLength(UserConstants.UserNameMaxLength, ErrorMessage = UserConstants.UserNameLengthErrorMessage, MinimumLength = UserConstants.UserNameMinLength)]
        [Comment("The username of the user")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = UserConstants.EmailRequiredMessage)]
        [EmailAddress(ErrorMessage = UserConstants.EmailFormatErrorMessage)]
        [Comment("The email address of the user")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = UserConstants.PasswordRequiredMessage)]
        [Comment("The hashed user password")]
        public string PasswordHashed { get; set; } = string.Empty;

        [Comment("Collection of user created events")]
        public ICollection<Event> CreatedEvents { get; set; } = new HashSet<Event>();

        [Comment("Collection of user participations in events")]
        public ICollection<Participant> Participations { get; set; } = new HashSet<Participant>();
    }
}
