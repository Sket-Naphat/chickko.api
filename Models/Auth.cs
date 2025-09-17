using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class LoginLog
    {
        [Key]
        public int LoginLogId { get; set; }

        [Required]
        public int? UserId { get; set; }
        public User? User { get; set; } = null!;

        public DateOnly LoginDate { get; set; }

        public TimeOnly LoginTime { get; set; }
    }
}