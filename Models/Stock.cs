using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace chickko.api.Models
{
    public class Stock //stock model main
    {
        [Key]
        public int StockId { get; set; } //base id ของ สิ่งของในร้าน
        [Required]
        public string ItemName { get; set; } = string.Empty; //ชื่อของสิ่งของนั้น
        [Required]
        public int StockCategoryID { get; set; }
        public StockCategory? StockCategory { get; set; } = null;
        public int RequiredQTY { get; set; } // จำนวนที่ต้องการ
        public int TotalQTY { get; set; } // จำนวนคงเหลือ
        public int StockInQTY { get; set; } //จำนวนที่ซื้อเพิ่ม
        public int StockUnitTypeID { get; set; }
        public StockUnitType? StockUnitType { get; set; }
        public DateOnly UpdateDate { get; set; } //วันที่อัพเดท วันที่แก้ไข stock ปัจจุบัน
        public TimeOnly UpdateTime { get; set; } //เวลาที่อัพเดท วันที่แก้ไข stock ปัจจุบัน
        public int StockLocationID { get; set; }
        public StockLocation? StockLocation { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public int? RecentStockLogId { get; set; }

        [ForeignKey("RecentStockLogId")]
        public StockLog? RecentStockLog { get; set; }
    }
    public class StockLog //stock log model สำหรับบันทึก stock รายวัน
    {
        [Key]
        public int StockLogId { get; set; }
        [Required]
        public int StockId { get; set; } //stock id
        [Required]
        public DateOnly StockInDate { get; set; } //วันที่ซื้อเข้า
        public TimeOnly StockInTime { get; set; } //เวลาซื้อเข้า
        public int RequiredQTY { get; set; } = 0!;// จำนวนที่ต้องการ
        public int TotalQTY { get; set; } = 0;// จำนวนคงเหลือ
        public int StockInQTY { get; set; } = 0;//จำนวนที่ต้องซื้อเพิ่ม
        public int PurcheseQTY { get; set; } = 0; //จำนวนที่ซื้อจริง
        public int DipQTY { get; set; } = 0;//จำนวนที่มันดิปกันอยู่ PurcheseQTY - StockInQTY
        public int Price { get; set; } = 0;//ราคาที่ซื้อ
        public bool IsPurchese { get; set; } = false;
        public int SupplyID { get; set; } = 0;
        public Supplier? Supplier { get; set; } = null;
        public string Remark { get; set; } = string.Empty;
        public int StockLogTypeID { get; set; } // 1 = Count, 2 = Purchase
        public StockLogType? StockLogType { get; set; }
        public int CostId { get; set; }
        public Cost? Cost { get; set; } = null;
    }
    public class Supplier
    {
        [Key]
        public int SupplyID { get; set; }
        public string SupplyName { get; set; } = string.Empty;
        public string SupplyContact { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    public class StockCategory
    {
        [Key]
        public int StockCategoryID { get; set; }
        public string StockCategoryName { get; set; } = ""!;
        public string Description { get; set; } = "";
    }
    public class StockUnitType
    {
        [Key]
        public int StockUnitTypeID { get; set; }
        public string StockUnitTypeName { get; set; } = ""!;
        public string Description { get; set; } = "";
    }
    public class StockLocation
    {
        [Key]
        public int StockLocationID { get; set; }
        public string StockLocationName { get; set; } = ""!;
        public string Description { get; set; } = "";
    }
    public class StockLogType
    {
        [Key]
        public int StockLogTypeID { get; set; }
        public string StockLogTypeName { get; set; } = ""!;
        public string Description { get; set; } = "";
    }
}