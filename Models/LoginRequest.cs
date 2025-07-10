namespace chickko.api.Models
{
    public class LoginRequest
    {
        public string Username { get; set; } = null!; // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
        public string Password { get; set; } = null!; // ใช้ null! เพื่อบอกว่าไม่ต้องการค่า null
    }
}