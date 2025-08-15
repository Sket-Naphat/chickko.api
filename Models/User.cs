using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime DateOfBirth { get; set; } // วันเกิดของผู้ใช้
        public DateTime StartWorkDate { get; set; } = DateTime.UtcNow; // วันที่เริ่มทำงาน
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // วันที่สร้างผู้ใช้
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // วันที่แก้ไขล่าสุด
        public bool IsActive { get; set; } = false; // สถานะการใช้งานของผู้ใช้ 
        public string Contact { get; set; } = "";
        public int UserPermistionID { get; set; } = 3;
        public UserPermistion? UserPermistion { get; set; } = null;
    }
    public class UserPermistion
    {
        [Key]
        public int UserPermistionID { get; set; }
        public string UserPermistionName { get; set; } = ""!;
        public string Description { get; set; } = "";
        public double WageCost { get; set; } = 50;
        
    }
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Start Work Date is required")]
        public DateTime StartWorkDate { get; set; } = DateTime.UtcNow;
    }

}