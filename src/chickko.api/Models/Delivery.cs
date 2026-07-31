using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class Delivery
    {
        [Key]
        public int DeliveryId { get; set; }
        public DateOnly SaleDate { get; set; } // 1. วันที่ขาย
        public decimal TotalSales { get; set; } // 2. ยอดขายรวม
        public decimal NetSales { get; set; } // 3. ยอดขายสุทธิหลังหัก GP
        public decimal GPPercent { get; set; } // 4. จำนวน GP ที่หักไปเป็น %
        public decimal GPAmount { get; set; } // 5. จำนวนเงินที่หัก GP ไปเป็นบาท
        public DateOnly UpdateDate { get; set; } // 6. วันที่อัปเดต
        public TimeOnly UpdateTime { get; set; } // 7. เวลาอัปเดต
        public int? UpdatedBy { get; set; } // 8. ผู้ที่อัปเดต
        public bool Active { get; set; } // 9. สถานะ Active
    }
}