public class DailySaleDto
{
    public DateOnly SaleDate { get; set; }   // วันที่ขาย
    public int Orders { get; set; }          // จำนวนบิล
    public decimal TotalAmount { get; set; } // ยอดขายรวม
    public double AvgPerOrder { get; set; }  // ค่าเฉลี่ยต่อบิล
}
public class IncomeDto
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

}
