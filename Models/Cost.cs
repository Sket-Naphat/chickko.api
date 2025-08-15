using System.ComponentModel.DataAnnotations;
using Google.Type;

namespace chickko.api.Models
{
    public class Cost
    {
        [Key]
        public int CostId { get; set; }
        [Required]
        public int CostCategoryID { get; set; } // id ประเภทค่าใช้จ่าย
        public CostCategory CostCategory { get; set; } = null!; //ประเภทค่าใช้จ่าย
        public double CostPrice { get; set; } = 0; //ราคาที่ซื้อ
        public string? CostDescription { get; set; } = string.Empty; //รายละเอียดการซื้อ
        public DateOnly? CostDate { get; set; } // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public TimeOnly? CostTime { get; set; }
        public bool IsPurchase { get; set; } = false; //ใชเพื่อบันทึกกรณีที่ยังไม่กดบันทึกแค่กรอกไว้เฉยๆ ให้เก็บค่าที่เคยกรอกไว้ไปแสดง
        public DateOnly? PurchaseDate { get; set; }
        public TimeOnly? PurchaseTime { get; set; }
        public int? CostStatusID { get; set; }
        public CostStatus? CostStatus { get; set; }
        public bool IsActive { get; set; } = true; // ใช้เพื่อบอกว่าต้นทุนนี้ยังใช้งานอยู่หรือไม่
        public int? CreateBy { get; set; } // ID ของผู้ที่สร้างต้นทุน
        public DateOnly? CreateDate { get; set; }
        public TimeOnly? CreateTime { get; set; }
        public int? UpdateBy { get; set; } // ID ของผู้ที่แก้ไขต้นทุน
        public DateOnly? UpdateDate { get; set; }
        public TimeOnly? UpdateTime { get; set; }

    }

    public class CostCategory
    {
        [Key]
        public int CostCategoryID { get; set; }
        public string CostCategoryName { get; set; } = string.Empty!;
        public string? Description { get; set; }
    }
    public class CostStatus
    {
        [Key]
        public int CostStatusID { get; set; }
        public string CostStatusName { get; set; } = ""!;
        public string Description { get; set; } = "";
    }
}