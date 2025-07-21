using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class ErrorLog
    {
        [Key]
        public int LogId { get; set; }
        public string? ErrorMassage { get; set; } = null!; // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public string? ErrorFile { get; set; }
        public DateOnly? ErrorDate { get; set; } // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public TimeOnly? ErrorTime { get; set; }

    }
}