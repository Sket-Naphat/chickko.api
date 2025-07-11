using System;
using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class OrderHeader
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        [Required]
        public int LocationOrderId { get; set; }  // Foreign Key
        public LocationOrder LocationOrder { get; set; } = null!; // Navigation Property
        [Required]
        public DateOnly OrderDate { get; set; }
        [Required]
        public TimeOnly OrderTime { get; set; }
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
        public string OrderRemark { get; set; } = string.Empty;
        public int? DiscountID { get; set; } = 0; // Foreign Key
        public Discount? Discount { get; set; } // Navigation Property for Discount
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
        public List<Menu>? Toppings { get; set; } // For multiple toppings
        public bool IsDone { get; set; } = false; // Indicates if the order detail is completed
        public bool IsDischarge { get; set; } = false; // Indicates if the order detail is discharged
        public string? Remark { get; set; } // Optional remark for the order detail

    }
    public class LocationOrder
    {
        [Key]
        public int LocationOrderId { get; set; }

        [Required]
        public string LocationName { get; set; } = string.Empty;

        public string? Description { get; set; }
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
}
