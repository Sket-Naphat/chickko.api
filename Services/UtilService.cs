using System;
using chickko.api.Data;
using chickko.api.Interface;
using chickko.api.Models;
using Google.Cloud.Firestore;

namespace chickko.api.Services
{
    public class UtilService : IUtilService
    {
        private readonly ChickkoContext _context;
        private readonly ISiteService _siteService;
        public UtilService(ChickkoContext context, ISiteService siteService)
        {
            _context = context;
            _siteService = siteService;
        }
        private FirestoreDb GetFirestoreDb()
        {
            try
            {
                var site = _siteService.GetCurrentSite();
                Console.WriteLine($"🔍 Current site: {site}");

                // ตั้งชื่อ environment variable ตาม site
                string envVarName = site switch
                {
                    "HKT" => "GOOGLE_APPLICATION_CREDENTIALS_JSON_HKT",
                    "BKK" => "GOOGLE_APPLICATION_CREDENTIALS_JSON_BKK"
                };

                Console.WriteLine($"🔍 Looking for environment variable: {envVarName}");
                var credentialsJson = Environment.GetEnvironmentVariable(envVarName);

                Console.WriteLine($"📋 Credentials found: {!string.IsNullOrEmpty(credentialsJson)}");
                
                if (!string.IsNullOrEmpty(credentialsJson))
                {
                    Console.WriteLine($"📊 Credentials length: {credentialsJson.Length}");
                    
                    // ใช้ชื่อไฟล์ที่แยกตาม site เพื่อหลีกเลี่ยงการทับกัน
                    var filePath = Path.Combine(Path.GetTempPath(), $"gcp-credentials-{site}.json");
                    Console.WriteLine($"📁 Writing credentials to: {filePath}");
                    
                    File.WriteAllText(filePath, credentialsJson);
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
                    Console.WriteLine("✅ Credentials file created successfully");
                }
                else
                {
                    Console.WriteLine($"❌ Environment variable {envVarName} not found or empty");
                    throw new Exception($"Missing {envVarName} environment variable");
                }

                // เลือก project ID ตาม site
                string projectId = site switch
                {
                    "HKT" => "chickkoapp",
                    "BKK" => "chick-ko-bkk"
                };

                Console.WriteLine($"🔥 Creating FirestoreDb with project: {projectId}");
                var firestoreDb = FirestoreDb.Create(projectId);
                Console.WriteLine("✅ FirestoreDb created successfully");
                
                return firestoreDb;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetFirestoreDb: {ex.Message}");
                throw;
            }
        }
        public async Task<QuerySnapshot> GetSnapshotFromFirestoreByCollectionName(string collectionName)
        {
            // สร้าง instance Firestore
            var db = GetFirestoreDb();

            // ดึง collection "orders"
            var ordersRef = db.Collection(collectionName);
            return await ordersRef.GetSnapshotAsync();
        }
        public async Task<QuerySnapshot> GetSnapshotFromFirestoreByCollectionNameAndOrderBy(string collectionName, string? orderByField = null)
        {
            // สร้าง instance Firestore
            var db = GetFirestoreDb();
            // ดึง collection "orders" และเรียงตาม field ที่ระบุ
            Query query = db.Collection(collectionName);

            if (!string.IsNullOrEmpty(orderByField))
                query = query.OrderBy(orderByField);

            return await query.GetSnapshotAsync();
        }

        public async Task<QuerySnapshot> GetSnapshotFromFirestoreWithFilters(string collectionName, string? orderByField = null, string? whereField = null, string? whereValue = null)
        {
            // สร้าง instance Firestore
            var db = GetFirestoreDb();
            // ดึง collection "orders" และเรียงตาม field ที่ระบุ
            Query query = db.Collection(collectionName);

            if (!string.IsNullOrEmpty(orderByField))
                query = query.OrderBy(orderByField);

            if (!string.IsNullOrEmpty(whereField) && !string.IsNullOrEmpty(whereValue))
                query = query.WhereEqualTo(whereField, whereValue);

            return await query.GetSnapshotAsync();
        }
        public async Task<QuerySnapshot> GetSnapshotFromFirestoreWithFiltersBetween(string collectionName, string? orderByField = null, string? whereField = null, string? whereValue = null, string? whereValue2 = null)
        {
            // สร้าง instance Firestore
            var db = GetFirestoreDb();
            // ดึง collection "orders" และเรียงตาม field ที่ระบุ
            Query query = db.Collection(collectionName);

            if (!string.IsNullOrEmpty(orderByField))
                query = query.OrderBy(orderByField);

            if (!string.IsNullOrEmpty(whereField) && !string.IsNullOrEmpty(whereValue) && !string.IsNullOrEmpty(whereValue2))
                query = query.WhereGreaterThanOrEqualTo(whereField, whereValue).WhereLessThanOrEqualTo(whereField, whereValue2);

            return await query.GetSnapshotAsync();
        }
        public async Task<QuerySnapshot> GetSnapshotFromFirestoreWithDateGreaterThan(
           string collectionName,
           string? orderByField = null,
           string? whereField = null,
           string? datefrom = null)
        {
            // สร้าง instance ของ Firestore
            var db = GetFirestoreDb();

            // เริ่มสร้าง query จาก collection ที่ระบุ
            Query query = db.Collection(collectionName);

            // ถ้ามีการระบุ orderByField ให้จัดเรียงผลลัพธ์ตาม field นั้น
            if (!string.IsNullOrEmpty(orderByField))
                query = query.OrderBy(orderByField);

            // ถ้ามีการระบุ whereField และ dateTo ให้กรองข้อมูลที่ whereField < dateTo
            if (!string.IsNullOrEmpty(whereField) && !string.IsNullOrEmpty(datefrom))
                query = query.WhereGreaterThan(whereField, datefrom);

            // ดึง snapshot จาก query ที่สร้าง
            return await query.GetSnapshotAsync();
        }
        public async Task<QuerySnapshot> GetSnapshotFromFirestoreWithID(string collectionName, string documentId)
        {
            var db = GetFirestoreDb();

            // ใช้ Query แทน DocumentRef เพื่อให้ได้ QuerySnapshot
            var query = db.Collection(collectionName)
                        .WhereEqualTo(FieldPath.DocumentId, documentId);

            return await query.GetSnapshotAsync();
        }

        public async Task AddErrorLog(ErrorLog errorLog)
        {
            _context.ErrorLogs.Add(errorLog);
            await _context.SaveChangesAsync();
        }

    }
}