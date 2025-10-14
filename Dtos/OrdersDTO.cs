public class DailySaleDto
{
    public DateOnly? SaleDate { get; set; } = null;  // วันที่ขาย
    public int? Orders { get; set; } = null;         // จำนวนบิล
    public decimal? TotalAmount { get; set; } = null; // ยอดขายรวม
    public decimal? TotalDiscount { get; set; } = null; // ยอดขายรวม
    public double? AvgPerOrder { get; set; } = null;  // ค่าเฉลี่ยต่อบิล
    public List<SoldMenuDto> TopSellingItems { get; set; } = new(); // รายการขายดี 5 อันดับ
    public int? totalOrders { get; set; } = null; // For total orders in the month
    public List<PeakHourDto> PeakHours { get; set; } = new(); // ช่วงเวลาที่ขายดี
}
// ✅ DTO ใหม่สำหรับช่วงเวลาที่ขายดี
public class PeakHourDto
{
    public string HourRange { get; set; } = string.Empty; // เช่น "12:00-13:00"
    public int OrderCount { get; set; } = 0; // จำนวนออเดอร์ในช่วงเวลานั้น
    public decimal TotalSales { get; set; } = 0; // ยอดขายรวมในช่วงเวลานั้น
    public double AvgPerOrder { get; set; } = 0; // ค่าเฉลี่ยต่อออเดอร์ในช่วงเวลานั้น
}
public class IncomeDto
{
    public int DeliveryId { get; set; }
    public DateOnly? SaleDate { get; set; } // 1. วันที่ขาย
    public decimal TotalSales { get; set; } // 2. ยอดขายรวม
    public decimal NetSales { get; set; } // 3. ยอดขายสุทธิหลังหัก GP
    public decimal GPPercent { get; set; } // 4. จำนวน GP ที่หักไปเป็น %
    public decimal GPAmount { get; set; } // 5. จำนวนเงินที่หัก GP ไปเป็นบาท
    public int? UpdatedBy { get; set; } // 8. ผู้ที่อัปเดต
    public string? SelectedMonth { get; set; } // For filtering by month
    public string? SelectedYear { get; set; } // For filtering by year
    public int? totalOrders { get; set; } // For total orders in the month
}

public class IncomeOrdersDTO
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly? OrderDate { get; set; }
    public TimeOnly? OrderTime { get; set; }
    public int OrderTypeId { get; set; }  // Foreign Key
    public string OrderTypeName { get; set; } = null!; // Navigation Property
    public TimeOnly? DischargeTime { get; set; }
    public bool IsDischarge { get; set; } = false;
    public TimeOnly? FinishOrderTime { get; set; }
    public bool IsFinishOrder { get; set; } = false;
    public decimal TotalPrice { get; set; }
    public decimal TotalGrabPrice { get; set; } = 0;
    public string OrderRemark { get; set; } = string.Empty;
    public int ItemQTY { get; set; }

    //list of order details
    public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    public decimal DiscountPrice { get; set; } = 0; // Price reduced by discount
    public int? DiscountID { get; set; } = 0; // Foreign Key
    public string? DiscountName { get; set; } = string.Empty; // Navigation Property for Discount

}
//รายการเมนูที่ขายได้
public class SoldMenuDto
{
    public int? MenuId { get; set; } = null;
    public string? MenuName { get; set; } = string.Empty;
    public int? QuantitySold { get; set; } = 0;
    public decimal? TotalSales { get; set; } = 0;
    public decimal? TotalCost { get; set; } = 0;
    public decimal? TotalProfit { get; set; } = 0;
    public double? ProfitMargin { get; set; } = 0;
}
