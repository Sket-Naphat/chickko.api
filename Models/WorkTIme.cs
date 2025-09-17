using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class Worktime
    {
        [Key]
        public int WorktimeID { get; set; }
        public DateOnly WorkDate { get; set; } = DateOnly.FromDateTime(System.DateTime.Now); //วันที่ทำงาน
        public TimeOnly? TimeClockIn { get; set; } //เวลาเข้างาน
        public TimeOnly? TimeClockOut { get; set; } //เวลาออกงาน
        [Required]
        public int EmployeeID { get; set; }
        public User Employee { get; set; } = null!;
        public string ClockInLocation { get; set; } = "";
        public string ClockOutLocation { get; set; } = "";
        public double TotalWorktime { get; set; } = 0;
        public double WageCost { get; set; } = 0!;
        public double Bonus { get; set; } = 0;
        public double Price { get; set; } = 0;
        public bool IsPurchase { get; set; } = false;
        public bool Active { get; set; } = true;
        public string Remark { get; set; } = "";
        public DateOnly? UpdateDate { get; set; }
        public TimeOnly? UpdateTime { get; set; }
        public int? UpdateBy { get; set; }
        public int? CostID { get; set; } = null;
        public Cost? Cost { get; set; } = null;
    }
}