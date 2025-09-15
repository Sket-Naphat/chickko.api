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

public class DeliveryOrdersDTO
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


public class OrderDetailDTO
{
    public int OrderDetailId { get; set; }
    public int OrderId { get; set; }  // Foreign Key
    public int MenuId { get; set; }  // Foreign Key
    public string MenuName { get; set; } = null!; // Navigation Property
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal GrabPrice { get; set; }
    public List<OrderDetailToppingDTO> Toppings { get; set; } = new();
    public int ToppingQTY { get; set; }
    public string? MenuIdInFirestore { get; set; } // Comma-separated list of topping IDs
    public bool IsDone { get; set; } = false; // Indicates if the order detail is completed
    public bool IsDischarge { get; set; } = false; // Indicates if the order detail is discharged
    public string? Remark { get; set; } // Optional remark for the order detail

}

public class OrderDetailToppingDTO
{
    public int OrderDetailToppingId { get; set; }

    public int OrderDetailId { get; set; }
    public OrderDetailDTO OrderDetail { get; set; } = null!;

    public int MenuId { get; set; } // ท็อปปิ้ง
    public string toppingNames { get; set; } = null!;

    public decimal ToppingPrice { get; set; } = 0;
    public decimal TotalGrabPrice { get; set; } = 0;
}
