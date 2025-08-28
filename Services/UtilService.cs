using System;
using chickko.api.Data;
using chickko.api.Interface;
using chickko.api.Models;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;
using Google.Cloud.Firestore.V1;

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
        // private FirestoreDb GetFirestoreDb()
        // {
        //     try
        //     {
        //         var site = _siteService.GetCurrentSite();
        //         Console.WriteLine($"🔍 Current site: {site}");

        //         // // ดึงค่า credentials จาก environment variable
        //         // // ตั้งชื่อ environment variable ตาม site
        //         // string envVarName = site switch
        //         // {
        //         //     "HKT" => "GOOGLE_APPLICATION_CREDENTIALS_JSON_HKT",
        //         //     "BKK" => "GOOGLE_APPLICATION_CREDENTIALS_JSON_BKK",
        //         //     _ => throw new ArgumentException("Unknown site")
        //         // };

        //         // Console.WriteLine($"🔍 Looking for environment variable: {envVarName}");
        //         // var credentialsJson = Environment.GetEnvironmentVariable(envVarName);

        //         // Console.WriteLine($"📋 Credentials found: {!string.IsNullOrEmpty(credentialsJson)}");

        //         // if (!string.IsNullOrEmpty(credentialsJson))
        //         // {
        //         //     Console.WriteLine($"📊 Credentials length: {credentialsJson.Length}");

        //         //     // ใช้ชื่อไฟล์ที่แยกตาม site เพื่อหลีกเลี่ยงการทับกัน
        //         //     var filePath = Path.Combine(Path.GetTempPath(), $"gcp-credentials-{site}.json");
        //         //     Console.WriteLine($"📁 Writing credentials to: {filePath}");

        //         //     File.WriteAllText(filePath, credentialsJson);
        //         //     Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
        //         //     Console.WriteLine("✅ Credentials file created successfully");
        //         // }
        //         // else
        //         // {
        //         //     Console.WriteLine($"❌ Environment variable {envVarName} not found or empty");
        //         //     throw new Exception($"Missing {envVarName} environment variable");
        //         // }

        //         // ตั้งค่า GOOGLE_APPLICATION_CREDENTIALS ตาม site
        //         switch (site)
        //         {
        //             case "HKT":
        //                 Environment.SetEnvironmentVariable(
        //         "GOOGLE_APPLICATION_CREDENTIALS",
        //         Path.Combine(Directory.GetCurrentDirectory(), "firebase/credentials.json")
        //       );
        //                 break;
        //             case "BKK":
        //                 Environment.SetEnvironmentVariable(
        //                     "GOOGLE_APPLICATION_CREDENTIALS",
        //                     Path.Combine(Directory.GetCurrentDirectory(), "firebase/credentials_bkk.json")
        //                 );
        //                 break;
        //             default:
        //                 throw new ArgumentException("Unknown site");
        //         }


        //         // เลือก project ID ตาม site
        //         string projectId = site switch
        //         {
        //             "HKT" => "chickkoapp",
        //             "BKK" => "chick-ko-bkk",
        //             _ => throw new ArgumentException("Unknown site")
        //         };

        //         Console.WriteLine($"🔥 Creating FirestoreDb with project: {projectId}");
        //         var firestoreDb = FirestoreDb.Create(projectId);
        //         Console.WriteLine("✅ FirestoreDb created successfully");

        //         return firestoreDb;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"❌ Error in GetFirestoreDb: {ex.Message}");
        //         throw;
        //     }
        // }
        private FirestoreDb GetFirestoreDb()
        {
            try
            {
                var site = _siteService.GetCurrentSite(); // "HKT" | "BKK"
                Console.WriteLine($"🔍 Current site: {site}");

                // project ที่คาดหวัง (กันพลาดกรณีในไฟล์ไม่มี project_id)
                var expectedProjectId = site switch
                {
                    "HKT" => "chickkoapp",
                    "BKK" => "chick-ko-bkk",
                    _ => throw new ArgumentException("Unknown site")
                };

                GoogleCredential credential;
                string projectId;

                // -----------------------------
                // ✅ โหมด Railway (ใช้ตัวแปรลับ JSON)
                //    ใส่ได้ทั้งตัวแปรรวม หรือแยกตามสาขา
                // -----------------------------
                var jsonEnvVarNamePerSite = $"GOOGLE_APPLICATION_CREDENTIALS_JSON_{site}";
                var jsonFromEnv =
                    Environment.GetEnvironmentVariable(jsonEnvVarNamePerSite) ??
                    Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON"); // fallback ชื่อรวม

                if (!string.IsNullOrWhiteSpace(jsonFromEnv))
                {
                    Console.WriteLine($"☁️  Using credentials from ENV: {jsonEnvVarNamePerSite} / GOOGLE_APPLICATION_CREDENTIALS_JSON");
                    credential = GoogleCredential.FromJson(jsonFromEnv)
                                                 .CreateScoped(FirestoreClient.DefaultScopes);

                    using var doc = JsonDocument.Parse(jsonFromEnv);
                    projectId = doc.RootElement.TryGetProperty("project_id", out var p)
                                ? p.GetString()!
                                : expectedProjectId;
                }
                else
                {
                    // -----------------------------
                    // 💻 โหมด Local (อ่านไฟล์ในโปรเจกต์)
                    // -----------------------------
                    var fileName = site == "BKK" ? "credentials_bkk.json" : "credentials.json";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "firebase", fileName);
                    if (!File.Exists(path))
                        throw new FileNotFoundException($"Credentials file not found: {path}");

                    Console.WriteLine($"💾 Using local credentials file: {path}");

                    credential = GoogleCredential.FromFile(path)
                                                 .CreateScoped(FirestoreClient.DefaultScopes);

                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    projectId = doc.RootElement.TryGetProperty("project_id", out var p)
                                ? p.GetString()!
                                : expectedProjectId;
                }

                // (optional) debug ว่าใช้ SA ตัวไหน
                if (credential.UnderlyingCredential is ServiceAccountCredential sac)
                    Console.WriteLine($"🔐 Service Account: {sac.Id}");

                // สร้าง FirestoreDb โดยกำหนด Credential ตรง ๆ (ไม่ใช้ ADC)
                Console.WriteLine($"🔥 Creating FirestoreDb with project: {projectId}");
                var db = new FirestoreDbBuilder
                {
                    ProjectId = projectId,
                    Credential = credential
                }.Build();

                Console.WriteLine("✅ FirestoreDb created successfully");
                return db;
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
            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetSnapshotFromFirestoreWithDateGreaterThan: {ex.Message}");
                throw;
            }
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