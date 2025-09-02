using chickko.api.Models;

namespace chickko.api.Interface
{
    public interface IAuthService
    {
        // ฟังก์ชันหลักที่ใช้จริง (รับ username/password ใช้ site จาก Header ผ่าน SiteService)
        Task<object> LoginAsync(string username, string password);
        Task<bool> Register(RegisterRequest request);

        // ===== ฟังก์ชันเดิม (ไม่ใช้แล้ว) คอมเมนต์เก็บไว้เผื่อย้อนกลับ =====
        // User? Login(string username, string password);
        // object LoginResponse(string username, string password);
        // object LoginResponseWithSite(string username, string password, string site);
        // string GenerateJwtToken(User user);
        // string GenerateJwtToken(User user, string site);
    }
}