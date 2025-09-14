public class DeliveryDto
{
    public int DeliveryId { get; set; }
    public DateOnly SaleDate { get; set; } // 1. วันที่ขาย
    public decimal TotalSales { get; set; } // 2. ยอดขายรวม
    public decimal NetSales { get; set; } // 3. ยอดขายสุทธิหลังหัก GP
    public decimal GPPercent { get; set; } // 4. จำนวน GP ที่หักไปเป็น %
    public decimal GPAmount { get; set; } // 5. จำนวนเงินที่หัก GP ไปเป็นบาท
    public int? UpdatedBy { get; set; } // 8. ผู้ที่อัปเดต
    public string? SelectedMonth { get; set; } // For filtering by month
    public string? SelectedYear { get; set; } // For filtering by year
}