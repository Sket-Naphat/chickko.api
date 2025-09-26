using System.ComponentModel.DataAnnotations;
using chickko.api.Models;

namespace chickko.api.Dtos
{
    public class CostDto
    {
        public int CostID { get; set; }
        public int CostCategoryID { get; set; } // id ประเภทค่าใช้จ่าย
        public CostCategory? costCategory { get; set; } = null!; // ประเภทค่าใช้จ่าย
        public double CostPrice { get; set; } = 0; //ราคาที่ซื้อ
        public string CostDescription { get; set; } = string.Empty; //รายละเอียดการซื้อ
        public DateOnly? CostDate { get; set; } // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public TimeOnly? CostTime { get; set; }
        public DateOnly? CreateDate { get; set; }
        public TimeOnly? CreateTime { get; set; }
        public DateOnly? UpdateDate { get; set; }
        public TimeOnly? UpdateTime { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public TimeOnly? PurchaseTime { get; set; }
        public bool IsPurchase { get; set; } = false; //ใชเพื่อบันทึกกรณีที่ยังไม่กดบันทึกแค่กรอกไว้เฉยๆ ให้เก็บค่าที่เคยกรอกไว้ไปแสดง
        public int? CostStatusID { get; set; } = null; // ใช้เพื่อบอกว่าจ่ายเงินแล้วหรือยัง ถ้าไม่ต้องการกรองให้ส่งค่า null
        public CostStatus? CostStatus { get; set; } = null!; // ใช้เพื่อบอกสถานะการจ่ายเงิน
        public bool? IsActive { get; set; } // ใช้เพื่อบอกว่าต้นทุนนี้ยังใช้งานอยู่หรือไม่
        public int? UpdateBy { get; set; } // ID ของผู้ที่แก้ไขต้นทุน
        public bool? IsStockIn { get; set; } // ใช้เพื่อบอกว่าต้นทุนนี้ถูกใช้ในการเพิ่มสต็อกหรือไม่
    }
    public class UpdateStockCostDto
    {
        public DateOnly? StockInDate { get; set; } = null!;
        public TimeOnly? StockInTime { get; set; } = null!;
        public double CostPrice { get; set; } = 0;
        public int CostID { get; set; } = 0;
        public int UpdateBy { get; set; } = 0;
        public bool IsPurchase { get; set; } = false;
        public string CostDescription { get; set; } = string.Empty;
    }
    public class GetCostListDto
    {
        public int? CostCategoryID { get; set; } = null; // id ประเภทค่าใช้จ่าย
        public int? CostStatusID { get; set; } = null; // ใช้เพื่อบอกว่าจ่ายเงินแล้วหรือยัง ถ้าไม่ต้องการกรองให้ส่งค่า null
        public DateOnly? StartDate { get; set; } = null!; // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public DateOnly? EndDate { get; set; } = null!;
        public bool? IsActive { get; set; } = true; // ใช้เพื่อบอกว่าต้นทุนนี้ยังใช้งานอยู่หรือไม่
        public bool? IsStockIn { get; set; } = null; // ใช้เพื่อบอกว่าต้นทุนนี้ถูกใช้ในการเพิ่มสต็อกหรือไม่
        public bool? IsPurchase { get; set; } = null; // ใช้เพื่อบอกว่าต้นทุนนี้ถูกใช้ในการซื้อหรือไม่
        public int? Month { get; set; } = null; // เดือน
        public int? Year { get; set; } = null; // ปี
    }
    public class SaleDateDto
    {
        public DateOnly? SaleDate { get; set; } = null!;
        public TimeOnly? SaleTime { get; set; } = null!;
        public int? Month { get; set; } = null; // เดือน
        public int? Year { get; set; } = null; // ปี
    }
    public class DailyCostReportDto
    {
        public DateOnly? CostDate { get; set; }
        public decimal TotalAmount { get; set; } // ยอดรวมทั้งหมดของวันนั้น
        public List<CostCategoryDetailDto> CategoryDetails { get; set; } = new List<CostCategoryDetailDto>();
        
        // Additional properties
        public string CostDateString => CostDate?.ToString("yyyy-MM-dd") ?? "";
        public int TotalCategories => CategoryDetails.Count;
        public int TotalItems => CategoryDetails.Sum(c => c.Count);
    }

    public class CostCategoryDetailDto
    {
        public int CostCategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal TotalAmount { get; set; } // ยอดรวมของหมวดนี้
        public int Count { get; set; } // จำนวนรายการในหมวดนี้
        
        // Additional properties
        public decimal Percentage { get; set; } // เปอร์เซ็นต์ของยอดรวมทั้งหมด
    }
}