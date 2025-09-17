using chickko.api.Data;
using chickko.api.Models;
using chickko.api.Interface;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace chickko.api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly ChickkoContext _context;
        private readonly ISiteService _siteService; // ใช้ดึง site ปัจจุบันจาก Header/Claim
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AuthService(IConfiguration config, ChickkoContext context, ISiteService siteService)
        {
            _config = config;
            _context = context;          // context นี้ถูกสร้างด้วย connection string ตาม site แล้ว (Program.cs)
            _siteService = siteService;
        }

        // ================== ฟังก์ชันหลักที่ใช้ (ใหม่) ===================
        public async Task<object> LoginAsync(string username, string password)
        {
            try
            {
                // อ่าน site จาก Header (ผ่าน SiteService) -> ถ้าไม่มี header จะ fallback Claim / DefaultSite
                var currentSite = _siteService.GetCurrentSite();

                // ดึงผู้ใช้จาก DB (ซึ่งคือ DB ของ site ปัจจุบัน)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                    return new { success = false, message = "Invalid username or password" };

                var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (verifyResult == PasswordVerificationResult.Failed)
                    return new { success = false, message = "Invalid username or password" };

                // (ออปชัน) บังคับว่าผู้ใช้ต้องอยู่ site เดียวกับ header
                // ถ้าไม่ต้องการเช็คให้ลบ if นี้
                if (!string.Equals(user.Site, currentSite, StringComparison.OrdinalIgnoreCase))
                {
                    return new { success = false, message = "User not allowed for this site" };
                }

                var token = GenerateJwtToken(user, currentSite);

                // บันทึก log การเข้าสู่ระบบ (ถ้าต้องการ)
                if (user.UserId != 1 && user.UserId != 3 && user.UserId != 4)
                {
                    var loginLog = new LoginLog
                    {
                        UserId = user.UserId,
                        LoginDate = DateOnly.FromDateTime(System.DateTime.Now),
                        LoginTime = TimeOnly.FromDateTime(System.DateTime.Now)
                    };
                    await InsertLoginLog(loginLog);
                }
                return new
                {
                    success = true,
                    token,
                    user = new
                    {
                        userId = user.UserId,
                        username = user.Username,
                        name = user.Name,
                        site = currentSite,
                        userPermistionID = user.UserPermistionID
                    }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "An error occurred during login.", error = ex.Message };
            }
        }

        // ================== ฟังก์ชันสร้าง JWT (ใช้ภายใน) ===================
        private string GenerateJwtToken(User user, string site)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("Site", site)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ================== Register (ยังใช้) ===================
        public async Task<bool> Register(RegisterRequest request)
        {
            var result = false;
            try
            {
                // เลือก site ที่จะบันทึก: ถ้า request.Site มีค่า ให้ใช้ค่านั้น, ถ้าไม่มีก็ใช้ currentSite จาก SiteService
                var selectedSite = !string.IsNullOrWhiteSpace(request.Site)
                    ? request.Site.ToUpperInvariant()
                    : _siteService.GetCurrentSite();

                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                    throw new Exception("Username already exists");

                var user = new User
                {
                    Username = request.Username,
                    Password = request.Password,
                    Name = request.Name,
                    DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc),
                    StartWorkDate = DateTime.SpecifyKind(request.StartWorkDate, DateTimeKind.Utc),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserPermistionID = 3,
                    Contact = "",
                    IsActive = true,
                    Site = selectedSite // ใช้ site ที่เลือกไว้
                };

                user.Password = _passwordHasher.HashPassword(user, request.Password);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new Exception("Registration failed: " + ex.Message);
            }
            return result;
        }
        public async Task InsertLoginLog(LoginLog loginLog)
        {
            _context.LoginLogs.Add(loginLog);
            await _context.SaveChangesAsync();
        }
        // ================== ฟังก์ชันเดิม (เลิกใช้ คอมเมนต์เก็บไว้) ===================
        /*
        public User? Login(string username, string password) { ... }
        public object LoginResponse(string username, string password) { ... }
        public object LoginResponseWithSite(string username, string password, string site) { ... }
        public string GenerateJwtToken(User user) { ... }
        public string GenerateJwtToken(User user, string site) { ... }
        */
    }
}