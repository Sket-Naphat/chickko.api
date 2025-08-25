using System.ComponentModel.DataAnnotations;

namespace chickko.api.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        // (เลิกใช้สำหรับเลือก DB เพราะใช้ Header แล้ว; คอมเมนต์ไว้)
        // public string Site { get; set; } = "HKT";
    }
}