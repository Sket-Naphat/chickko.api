using System;
using chickko.api.Interface;
using Google.Cloud.Firestore;

namespace chickko.api.Services
{
    public class UtilService : IUtilService
    {
        private FirestoreDb GetFirestoreDb()
        {
            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                Path.Combine(Directory.GetCurrentDirectory(), "firebase/credentials.json")
            );

            return FirestoreDb.Create("chickkoapp");
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
    }
}