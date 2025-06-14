using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AiCalendar.WebApi.Models
{
    [PrimaryKey(nameof(UserId), nameof(EventId))]
    public class Participant
    {
        [Required]
        [Comment("The id of the user who participate in the event")]
        public Guid UserId { get; set; }

        [Required]
        [ForeignKey(nameof(UserId))]
        [Comment("User navigation property")]
        public User User { get; set; } = null!;

        [Required]
        [Comment("The id of the event that user participate")]
        public Guid EventId { get; set; }

        [Required]
        [ForeignKey(nameof(EventId))]
        [Comment("Event navigation property")]
        public Event Event { get; set; } = null!;
    }
}
