using System.ComponentModel.DataAnnotations;

namespace chickko.api.Dtos
{
    public class WorktimeDto
    {
        public int WorktimeID { get; set; }
        public string? WorkDate { get; set; }
        public string? TimeClockIn { get; set; }
        public string? TimeClockOut { get; set; }
        public string ClockInLocation { get; set; } = "";
        public double TotalWorktime { get; set; } = 0;
        public double WageCost { get; set; } = 0!;
        public double Bonus { get; set; } = 0;
        public double Price { get; set; } = 0;
        public bool IsPurchase { get; set; } = false;
        public string Remark { get; set; } = "";
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = ""!;
        public bool Active { get; set; }
        public string? WorkMonth { get; set; }
        public string? WorkYear { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? PurchaseDate { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class WorktimeSummaryDto
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; } = ""!;
        public double TotalWorktime { get; set; } = 0;
        public double WageCost { get; set; } = 0;
        
        public List<WorktimeDto> Worktimes { get; set; } = new();
    }
}
