using System.ComponentModel.DataAnnotations;

namespace chickko.api.Dtos
{
    public class StockDto
    {
        public int StockId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int StockCategoryID { get; set; }
        public string StockCategoryName { get; set; } = string.Empty;
        public int StockUnitTypeID { get; set; }
        public string StockUnitTypeName { get; set; } = string.Empty;
        public int StockLocationID { get; set; }
        public string StockLocationName { get; set; } = string.Empty;
        public int TotalQTY { get; set; }
        public int RequiredQTY { get; set; }
        public int StockInQTY { get; set; }
        public int PurchaseQTY { get; set; }
        public string UpdateDate { get; set; } = string.Empty;
        public string UpdateTime { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        public int? RecentStockLogId { get; set; }
        public int SupplyId { get; set; } = 0;
        public int Price { get; set; } = 0;
        public bool Active { get; set; } = false; // ใช้เพื่อบอกว่าการบันทึกนี้ยังใช้งานอยู่หรือไม่
    }
    public class StockCountDto
    {
        public int StockLogId { get; set; } // ID ของ StockLog
        public int StockId { get; set; } //id stock item
        public string StockInDate { get; set; } = DateOnly.FromDateTime(System.DateTime.Now).ToString("yyyy-MM-dd");
        public string StockInTime { get; set; } = TimeOnly.FromDateTime(System.DateTime.Now).ToString("HH:mm:ss");
        public string StockCountDate { get; set; } = DateOnly.FromDateTime(System.DateTime.Now).ToString("yyyy-MM-dd");
        public string StockCountTime { get; set; } = TimeOnly.FromDateTime(System.DateTime.Now).ToString("HH:mm:ss");
        public int TotalQTY { get; set; } //จำนวนคงเหลือ
        public int? RequiredQTY { get; set; } = null; //มีไว้เผื่อแก้ไขจำนวนที่ต้องใช้
        public int? StockInQTY { get; set; } = null; //มีไว้เผื่อแก้ไขจำนวนที่ต้องซื้อมากกว่าปกติ
        public string Remark { get; set; } = string.Empty;
        public int? UpdateBy { get; set; } // ID ของผู้ที่แก้ไขต้นทุน
        public int SupplyId { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public int? StockCategoryID { get; set; } = null;
        public string? StockCategoryName { get; set; } = string.Empty;
        public int? StockUnitTypeID { get; set; } = null;
        public string? StockUnitTypeName { get; set; } = string.Empty;
        public int? StockLocationID { get; set; } = null;
        public string? StockLocationName { get; set; } = string.Empty;
        public int? CostId { get; set; } = null; // ID ของต้นทุนที่เกี่ยวข้อง
        public int? StockLogTypeID { get; set; } = null; // ประเภทของการบันทึก
        public int? PurchaseQTY { get; set; } = null; // จำนวนที่ซื้อจริง
    }
    public class StockInDto
    {
        [Required]
        public int StockId { get; set; }
        public string? StockName { get; set; } = string.Empty;
        [Required]
        public int StockLogId { get; set; } // ID ของ StockLog
        [Required]
        public string StockInDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public string StockInTime { get; set; } = DateTime.Now.ToString("HH:mm:ss");
        [Required]
        public int StockInQTY { get; set; }
        public int PurchaseQTY { get; set; } = 0;
        public int Price { get; set; } = 0;
        public bool IsPurchase { get; set; } = false;
        public int SupplyId { get; set; } = 0;
        public string Remark { get; set; } = string.Empty;
        public int? CostId { get; set; }
        public bool IsStockIn { get; set; } = false; // ใช้เพื่อบอกว่าการซื้อเข้าเป็นการซื้อจริงหรือไม่
        public int? UpdateBy { get; set; } = 0; // ID ของผู้ที่แก้ไขต้นทุน
    }
    public class GetStockCountLogByCostId
    {
        public double CostPrice { get; set; } = 0;
        public string StockInDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public List<StockCountDto> StockCountDtos { get; set; } = new List<StockCountDto>();
    }

    public class UpdateStockInCostDto
    {
        public UpdateStockCostDto UpdateStockCostDto { get; set; } = null!;
        public List<StockInDto> StockInDto { get; set; } = null!;
    }

}