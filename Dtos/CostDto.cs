
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
}