using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = null!;

        [MaxLength(255)]
        public string Description { get; set; } = null!;
    }
}