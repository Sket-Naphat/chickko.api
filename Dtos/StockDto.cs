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
    }
    public class StockCountDto
    {
        [Required]
        public int StockId { get; set; } //id stock item
        [Required]
        public string StockInDate { get; set; } = DateOnly.FromDateTime(System.DateTime.Now).ToString("yyyy-MM-dd");
        public string StockInTime { get; set; } = TimeOnly.FromDateTime(System.DateTime.Now).ToString("HH:mm:ss");
        [Required]
        public int TotalQTY { get; set; } //จำนวนคงเหลือ
        public int? RequiredQTY { get; set; } = null; //มีไว้เผื่อแก้ไขจำนวนที่ต้องใช้
        public int? StockInQTY { get; set; } = null; //มีไว้เผื่อแก้ไขจำนวนที่ต้องซื้อมา่กกว่าปกติ
        public string Remark { get; set; } = string.Empty;
        public int? UpdateBy { get; set; } // ID ของผู้ที่แก้ไขต้นทุน
        public int SupplyId { get; set; } = 0;
    }
    public class StockInDto
    {
        [Required]
        public int StockId { get; set; }
        [Required]
        public string StockInDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
        public string StockInTime { get; set; } = DateTime.Now.ToString("HH:mm:ss");
        [Required]
        public int StockInQTY { get; set; }
        public int PurchaseQTY { get; set; } = 0;
        public int Price { get; set; } = 0;
        public int SupplyId { get; set; } = 0;
        public string Remark { get; set; } = string.Empty;
    }

}