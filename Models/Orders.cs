using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace chickko.api.Models
{
    public class OrderHeader
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        [Required]
        public DateOnly? OrderDate { get; set; }
        [Required]
        public TimeOnly? OrderTime { get; set; }
        [Required]
        public int OrderTypeId { get; set; }  // Foreign Key
        public Ordertype OrderType { get; set; } = null!; // Navigation Property
        [Required]
        public int DischargeTypeId { get; set; }  // Foreign Key
        public DischargeType DischargeType { get; set; } = null!; // Navigation Property
        public TimeOnly? DischargeTime { get; set; }
        public bool IsDischarge { get; set; } = false;
        public TimeOnly? FinishOrderTime { get; set; }
        public bool IsFinishOrder { get; set; } = false;
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }
        public decimal TotalGrabPrice { get; set; } = 0;
        public string OrderRemark { get; set; } = string.Empty;
        public int? DiscountID { get; set; } = 0; // Foreign Key
        public Discount? Discount { get; set; } // Navigation Property for Discount
        public string? IdInFirestore { get; set; } // Optional field to store Firestore ID
        public int? TableID { get; set; } // Optional field for table number
        public Table? Table { get; set; } // Navigation Property for Table
        public int ItemQTY { get; set; }
    }

    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        public int OrderId { get; set; }  // Foreign Key

        public OrderHeader OrderHeader { get; set; } = null!; // Navigation Property

        [Required]
        public int MenuId { get; set; }  // Foreign Key

        public Menu Menu { get; set; } = null!; // Navigation Property

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public List<OrderDetailTopping> Toppings { get; set; } = new();
        public int ToppingQTY { get; set; }
        public string? MenuIdInFirestore { get; set; } // Comma-separated list of topping IDs
        public bool IsDone { get; set; } = false; // Indicates if the order detail is completed
        public bool IsDischarge { get; set; } = false; // Indicates if the order detail is discharged
        public string? Remark { get; set; } // Optional remark for the order detail

    }
    public class Ordertype
    {
        [Key]
        public int OrderTypeId { get; set; }

        [Required]
        public string OrderTypeName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
    public class DischargeType
    {
        [Key]
        public int DischargeTypeId { get; set; }

        [Required]
        public string DischargeName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
    public class Discount
    {
        [Key]
        public int DiscountID { get; set; }

        [Required]
        public string DiscountName { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        public string? Description { get; set; }
    }
    public class Table
    {
        [Key]
        public int TableID { get; set; }

        [Required]
        public string TableName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class CopyOrderFromFirestore
    {
        public string OrderDateFrom { get; set; } = string.Empty;
        public string OrderDateTo { get; set; } = string.Empty;
    }
    public class OrderDetailTopping
    {
        public int OrderDetailToppingId { get; set; }

        [Required]
        public int OrderDetailId { get; set; }
        public OrderDetail OrderDetail { get; set; } = null!;

        [Required]
        public int MenuId { get; set; } // ท็อปปิ้ง
        public Menu Menu { get; set; } = null!;

        public decimal ToppingPrice { get; set; } = 0;
    }
    public class ImportOrderExcel
    {
        public string? menu_name { get; set; }
        public string? customer_name { get; set; }
        public string? order_date { get; set; }
        public string? order_time { get; set; }
        public string? finish_order_time { get; set; }
        public string? discharge_time { get; set; }
        public string? discharge_type { get; set; }
        public string? price { get; set; }
        public string? promptpay_price { get; set; }
        public string? cash_price { get; set; }
        public string? unit_price { get; set; }
        public string? qty { get; set; }
        public string? cost { get; set; }
        public string? unit_cost { get; set; }
        public string? profit { get; set; }
    }
}
