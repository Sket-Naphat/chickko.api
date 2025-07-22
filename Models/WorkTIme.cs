using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class WorkTime
    {
        [Key]
        public int WorkTimeID { get; set; }
        public DateOnly WorkDate { get; set; } = DateOnly.FromDateTime(System.DateTime.Now);
        public TimeOnly TimeClockIn { get; set; }
        public TimeOnly TimeClockOut { get; set; }
        public string ClockInLocation { get; set; } = "";
        public int TotalWorkTime { get; set; }
        public int wage { get; set; } = 0!;
        public int bonus { get; set; } = 0;
        public int Price { get; set; } = 0;
        public bool IsPurchese { get; set; } = false;
        public bool Active { get; set; } = true;
        public string Remark { get; set; } = ""; 
    }
}