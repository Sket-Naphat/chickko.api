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
        public UtilService(ChickkoContext context)
        {
            _context = context;
        }
        private FirestoreDb GetFirestoreDb()
        {
            try
            {
                Console.WriteLine("🔍 กำลังตรวจสอบ Environment Variables...");
                
                // ตรวจสอบว่ามี environment variable หรือไม่
                var credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON");
                
                Console.WriteLine($"📋 GOOGLE_APPLICATION_CREDENTIALS_JSON exists: {!string.IsNullOrEmpty(credentialsJson)}");
                if (!string.IsNullOrEmpty(credentialsJson))
                {
                    Console.WriteLine($"� Credentials length: {credentialsJson.Length}");
                    Console.WriteLine($"📝 First 50 chars: {(credentialsJson.Length > 50 ? credentialsJson.Substring(0, 50) + "..." : credentialsJson)}");
                    
                    // ตรวจสอบว่าเป็น JSON ที่ถูกต้องหรือไม่
                    try
                    {
                        System.Text.Json.JsonDocument.Parse(credentialsJson);
                        Console.WriteLine("✅ JSON format is valid");
                    }
                    catch (Exception jsonEx)
                    {
                        Console.WriteLine($"❌ Invalid JSON format: {jsonEx.Message}");
                        throw new Exception($"Invalid JSON credentials: {jsonEx.Message}");
                    }
                    
                    var filePath = Path.Combine(Path.GetTempPath(), "gcp-credentials.json");
                    Console.WriteLine($"📁 Writing credentials to: {filePath}");
                    
                    File.WriteAllText(filePath, credentialsJson);
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
                    
                    Console.WriteLine("✅ Credentials file created successfully");
                }
                else
                {
                    Console.WriteLine("❌ GOOGLE_APPLICATION_CREDENTIALS_JSON not found or empty");
                    throw new Exception("Missing GOOGLE_APPLICATION_CREDENTIALS_JSON environment variable");
                }

                Console.WriteLine("🔥 Creating FirestoreDb instance...");
                var db = FirestoreDb.Create("chickkoapp");
                Console.WriteLine("✅ FirestoreDb created successfully");
                
                return db;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetFirestoreDb: {ex.Message}");
                Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
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