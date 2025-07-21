using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class Cost
    {
        [Key]
        public int CostId { get; set; }
        [Required]
        public int CostCategoryID { get; set; }
        public CostCategory CostCategory { get; set; } = null!;
        public int CostPrice { get; set; } = 0;
        public string CostDescription { get; set; } = string.Empty;
        public DateOnly? CostDate { get; set; } // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public TimeOnly? CostTime { get; set; }
    }

    public class CostCategory
    {
        [Key]
        public int CostCategoryID { get; set; }
        public string CostCategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}