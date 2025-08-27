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
        
        public FirestoreService(ISiteService siteService, ILogger<FirestoreService> logger)
        {
            _siteService = siteService;
            _logger = logger;
        }

        private void InitializeFirebase(string site)
        {
            if (_isInitialized) return;

            lock (_lockObj)
            {
                if (_isInitialized) return;

                try 
                {
                    string envName = site switch
                    {
                        "HKT" => "GOOGLE_APPLICATION_CREDENTIALS_JSON_HKT",
                        "BKK" => "GOOGLE_APPLICATION_CREDENTIALS_JSON_BKK",
                        _ => throw new Exception($"ไม่รู้จัก site: {site}")
                    };

                    var credentialsJson = Environment.GetEnvironmentVariable(envName);
                    if (string.IsNullOrEmpty(credentialsJson))
                    {
                        throw new Exception($"ไม่พบ credentials สำหรับ site: {site}");
                    }

                    var filePath = Path.Combine(Path.GetTempPath(), $"gcp-credentials-{site}.json");
                    File.WriteAllText(filePath, credentialsJson);
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);

                    if(site == "HKT")
                    {
                        _db = FirestoreDb.Create("chickkoapp"); // แก้เป็น project ID จริง
                    }
                    else if(site == "BKK")
                    {
                        _db = FirestoreDb.Create("chick-ko-bkk"); // แก้เป็น project ID จริง
                    }
                    
                    _isInitialized = true;
                    
                    _logger.LogInformation($"✅ เชื่อมต่อ Firestore สำเร็จ (site: {site})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ ไม่สามารถเชื่อมต่อ Firestore ได้ (site: {site})");
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
