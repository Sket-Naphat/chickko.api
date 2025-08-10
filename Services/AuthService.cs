using chickko.api.Data;
using chickko.api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // สำหรับ PasswordHasher

namespace chickko.api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly ChickkoContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AuthService(IConfiguration config, ChickkoContext context)
        {
            _config = config;
            _context = context; // ใช้ ChickkoContext เพื่อเข้าถึงฐานข้อมูล
            
        }

        // ฟังก์ชัน Login ตัวอย่าง (ไม่มี DB ใช้ค่าคงที่)
        public User? Login(string username, string password)
        {
            // ตรวจสอบ username และ password
            var user = _context.Users.FirstOrDefault(u => u.Username == username );
            if (user == null)
            {
                
                return null; // ถ้าไม่พบผู้ใช้
            }
            // ตรวจสอบรหัสผ่าน
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return null; // ถ้ารหัสผ่านไม่ถูกต้อง
            }

            // ถ้าผู้ใช้และรหัสผ่านถูกต้อง คืนค่าผู้ใช้

            return user; // คืนค่าผู้ใช้ที่เข้าสู่ระบบสำเร็จ    
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"], // กำหนด Issuer
                audience: _config["Jwt:Audience"], // กำหนด Audience 
                claims: claims,// กำหนด Claims
                notBefore: DateTime.Now, // กำหนดเวลาเริ่มต้นของ Token         
                expires: DateTime.Now.AddHours(12),  // กำหนดเวลา Expiration ของ Token    
                signingCredentials: creds   // กำหนด Signing Credentials 
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
       public async Task<User> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new Exception("Username already exists");
            }

            var user = new User
            {
                Username = request.Username,
                Password = request.Password,
                Name = request.Name,
                Role = "user",
                DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc),
                StartWorkDate = DateTime.SpecifyKind(request.StartWorkDate, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            user.Password = _passwordHasher.HashPassword(user, request.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}