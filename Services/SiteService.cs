using chickko.api.Interface;

namespace chickko.api.Services
{
    /// <summary>
    /// จัดการการระบุ Site ปัจจุบันของ request
    /// ลำดับการหา:
    /// 1. JWT Claim "Site"
    /// 2. ค่า DefaultSite ใน appsettings (fallback)
    /// </summary>
    public class SiteService : ISiteService
    {
        // IConfiguration สำหรับอ่านค่า ConnectionStrings / DefaultSite
        private readonly IConfiguration _configuration;

        // ใช้อ่าน HttpContext เพื่อเข้าถึง User (Claims)
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _cachedSite;

        public SiteService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// คืนค่า site ปัจจุบัน (HKT / BKK) จาก JWT claim ถ้าไม่มีใช้ DefaultSite
        /// </summary>
        public string GetCurrentSite()
        {
            if (_cachedSite != null) return _cachedSite;

            var http = _httpContextAccessor.HttpContext
                       ?? throw new InvalidOperationException("No HttpContext.");

            // 1) ถ้ามี token แล้ว ใช้ claim ก่อน (ป้องกัน header แอบเปลี่ยน)
            var claimSite = Normalize(http.User?.FindFirst("Site")?.Value);
            if (IsValid(claimSite))
            {
                _cachedSite = claimSite!;
                return _cachedSite;
            }

            // 2) (กรณียังไม่ login) ใช้ header เพื่อบอกว่าจะเข้า DB ไหน
            string? headerSite = null;
            if (http.Request.Headers.TryGetValue("X-Site", out var v1))
                headerSite = v1.FirstOrDefault();
            else if (http.Request.Headers.TryGetValue("Site", out var v2))
                headerSite = v2.FirstOrDefault();

            headerSite = Normalize(headerSite);
            if (IsValid(headerSite))
            {
                _cachedSite = headerSite!;
                return _cachedSite;
            }

            // 3) ถ้าไม่มีทั้ง claim และ header => แจ้งให้ client ส่ง header
            throw new InvalidOperationException("Missing site (add X-Site header).");
        }

        /// <summary>
        /// คืนค่า connection string ตาม site ปัจจุบัน
        /// ถ้า site ปัจจุบันไม่มี connection string จะ fallback เป็น HKT
        /// </summary>
        public string GetConnectionString()
        {
            var site = GetCurrentSite();
            var conn = _configuration.GetConnectionString(site);
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException($"Connection string for site '{site}' not found.");
            return conn;
        }

        // Helpers
        private static string? Normalize(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToUpperInvariant();

        private static bool IsValid(string? s)
            => s == "HKT" || s == "BKK";
    }
}