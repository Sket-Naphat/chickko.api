using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace chickko.api.Models
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool Active { get; set; } = true;

        public bool IsTopping { get; set; } = false;

        [ForeignKey("Category")]
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }
        public string IdInFirestore { get; set; } = string.Empty;
    }
}
