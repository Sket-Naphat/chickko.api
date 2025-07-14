using System;
using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class Stock //stock model
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ItemName { get; set; } = string.Empty;
        [Required]
        public int RequiredQuantity { get; set; }

        public string Remark { get; set; } = string.Empty;
    }
    public class StockLog //stock log model
    {
        [Key]
        public int Id { get; set; }
        public Stock Stock { get; set; } = null!;
        [Required]
        public int StockId { get; set; }
        [Required]
        public DateOnly StockInDate { get; set; }
        public int RemainingQuantity { get; set; }
        public int QuantityToPurchase { get; set; }
        public int StockInQuantity { get; set; }
    }
}