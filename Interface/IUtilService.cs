using System;
using Google.Cloud.Firestore;

namespace chickko.api.Interface
{
    public interface IUtilService
    {
        Task<QuerySnapshot> GetSnapshotFromFirestoreByCollectionName(string collectionName);
        Task<QuerySnapshot> GetSnapshotFromFirestoreByCollectionNameAndOrderBy(string collectionName, string? orderByField = null);
        Task<QuerySnapshot> GetSnapshotFromFirestoreWithFilters(string collectionName, string? orderByField = null, string? whereField = null, string? whereValue = null);
        Task<QuerySnapshot> GetSnapshotFromFirestoreWithFiltersBetween(string collectionName, string? orderByField = null, string? whereField = null, string? whereValue = null, string? whereValue2 = null);
        Task<QuerySnapshot> GetSnapshotFromFirestoreWithDateLessThan(string collectionName, string? orderByField = null, string? whereField = null, string? dateTo = null);
        Task<QuerySnapshot> GetSnapshotFromFirestoreWithID(string collectionName, string DocumentId);
    }
}