using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using chickko.api.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace chickko.api.Services
{
    /// <summary>
    /// Service สำหรับจัดการการเชื่อมต่อ Firestore
    /// </summary>
    public class FirestoreService
    {
        private readonly ISiteService _siteService;
        private readonly ILogger<FirestoreService> _logger;
        private FirestoreDb? _db;
        private bool _isInitialized;
        private readonly object _lockObj = new object();
        private string? _siteInUse;
        
        public FirestoreService(ISiteService siteService, ILogger<FirestoreService> logger)
        {
            _siteService = siteService;
            _logger = logger;
        }

        private void InitializeFirebase(string site)
        {
            // ถ้า init แล้วและ site เดิม ให้ข้าม
            if (_isInitialized && string.Equals(_siteInUse, site, StringComparison.OrdinalIgnoreCase)) return;

            lock (_lockObj)
            {
                if (_isInitialized && string.Equals(_siteInUse, site, StringComparison.OrdinalIgnoreCase)) return;

                try
                {
                    if (string.IsNullOrWhiteSpace(site))
                        throw new Exception("site ว่างเปล่า");

                    var normalizedSite = site.Trim().ToUpperInvariant();

                    // เลือกชื่อ ENV ตาม site พร้อม fallback ชื่อรวม
                    string[] envCandidates = normalizedSite switch
                    {
                        "HKT" => new[] { "GOOGLE_APPLICATION_CREDENTIALS_JSON_HKT", "GOOGLE_APPLICATION_CREDENTIALS_JSON" },
                        "BKK" => new[] { "GOOGLE_APPLICATION_CREDENTIALS_JSON_BKK", "GOOGLE_APPLICATION_CREDENTIALS_JSON" },
                        _ => throw new Exception($"ไม่รู้จัก site: {site}")
                    };

                    string? credentialsJson = null;
                    string? chosenEnv = null;
                    foreach (var name in envCandidates)
                    {
                        var v = Environment.GetEnvironmentVariable(name);
                        if (!string.IsNullOrWhiteSpace(v))
                        {
                            credentialsJson = v;
                            chosenEnv = name;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(credentialsJson))
                        throw new Exception($"ไม่พบ credentials สำหรับ site: {site}. โปรดตั้งค่า ENV: {string.Join(", ", envCandidates)}");

                    // ใช้ Credential จาก JSON โดยตรง (ไม่ต้องเขียนไฟล์/ตั้ง ENV ทั่วระบบ)
                    var credential = GoogleCredential.FromJson(credentialsJson);

                    string projectId = normalizedSite switch
                    {
                        "HKT" => "chickkoapp",
                        "BKK" => "chick-ko-bkk",
                        _ => throw new Exception($"ไม่รู้จัก site: {site}")
                    };

                    _db = new FirestoreDbBuilder
                    {
                        ProjectId = projectId,
                        Credential = credential
                    }.Build();

                    _siteInUse = normalizedSite;
                    _isInitialized = true;

                    _logger.LogInformation("✅ เชื่อมต่อ Firestore สำเร็จ (site: {site}, env: {env}, project: {project})", normalizedSite, chosenEnv, projectId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ไม่สามารถเชื่อมต่อ Firestore ได้ (site: {site})", site);
                    throw;
                }
            }
        }

        /// <summary>
        /// ดึงข้อมูล Orders จาก Firestore ตามช่วงวันที่
        /// </summary>
        public async Task<QuerySnapshot> GetOrdersAsync(string dateFrom = "", string dateTo = "")
        {
            var site = _siteService.GetCurrentSite();
            InitializeFirebase(site);

            if (_db == null)
                throw new Exception("Firestore ยังไม่พร้อมใช้งาน");

            try
            {
                Query query = _db.Collection("orders");

                // ถ้ามีวันที่เริ่มต้น
                if (!string.IsNullOrEmpty(dateFrom))
                {
                    query = query.WhereGreaterThanOrEqualTo("orderDate", dateFrom);
                }

                // ถ้ามีวันที่สิ้นสุด
                if (!string.IsNullOrEmpty(dateTo))
                {
                    query = query.WhereLessThanOrEqualTo("orderDate", dateTo);
                }

                // เรียงตามวันที่และเวลา
                query = query.OrderBy("orderDate").OrderBy("orderTime");

                var snapshot = await query.GetSnapshotAsync();
                _logger.LogInformation($"✅ ดึงข้อมูล orders สำเร็จ: {snapshot.Documents.Count} รายการ");
                
                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ไม่สามารถดึงข้อมูล orders ได้");
                throw;
            }
        }

        // เพิ่มเติมเมธอดสำหรับดึงข้อมูลเมนู
        public async Task<QuerySnapshot> GetMenusAsync()
        {
            var site = _siteService.GetCurrentSite();
            InitializeFirebase(site);
            if (_db == null)
            {
                throw new Exception("Firestore ยังไม่พร้อมใช้งาน");
            }

            var collection = _db.Collection("menu");
            var query = collection.OrderBy("category");
            return await query.GetSnapshotAsync();
        }
    }
}
