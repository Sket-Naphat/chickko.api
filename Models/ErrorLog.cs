using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class ErrorLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; } = "";

        public string? StackTrace { get; set; }

        public string? Source { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Path { get; set; }  // เช่น "/api/stock/create"

        public string? Method { get; set; } // GET/POST
    }

}