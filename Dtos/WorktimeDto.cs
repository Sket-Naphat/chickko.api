using System.ComponentModel.DataAnnotations;

namespace chickko.api.Dtos
{
    public class WorktimeDto
    {
        public int WorktimeID { get; set; }
        [Required]
        public string WorkDate { get; set; } = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd"); // ⬅️ วันที่ปัจจุบัน
        public string? TimeClockIn { get; set; } = TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm:ss"); // ⬅️ เวลาเข้างาน (ตอนนี้)
        public string? TimeClockOut { get; set; } = TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm:ss"); // ⬅️ เวลาออกงาน (ตอนนี้)

        public string ClockInLocation { get; set; } = "";
        public double TotalWorktime { get; set; } = 0;
        public double wage { get; set; } = 0!;
        public double bonus { get; set; } = 0;
        public double Price { get; set; } = 0;
        public bool IsPurchese { get; set; } = false;
        public string Remark { get; set; } = "";
        [Required]
        public int EmployeeID { get; set; }
    }
}
