using System.Linq;
using chickko.api.Data;
using chickko.api.Interface;
using chickko.api.Models;
using Microsoft.EntityFrameworkCore;

namespace chickko.api.Services
{
    public class MenuService : IMenuService
    {
        private readonly ChickkoContext _context;
        private readonly ILogger<MenuService> _logger;
        private readonly IUtilService _utilService;
        private readonly FirestoreService _firestoreService;

        public MenuService(ChickkoContext context, ILogger<MenuService> logger, IUtilService utilService, FirestoreService firestoreService)
        {
            _context = context;
            _logger = logger;
            _utilService = utilService;
            _firestoreService = firestoreService;
        }

        public List<Menu> GetMenus()
        {
            return _context.Menus.ToList();
        }

        public Menu? GetMenu(int id)
        {
            return _context.Menus.Find(id);
        }

        public Menu CreateMenu(Menu menu)
        {
            _context.Menus.Add(menu);
            _context.SaveChanges();
            return menu;
        }

        public Menu? UpdateMenu(int id, Menu menu)
        {
            var existing = _context.Menus.Find(id);
            if (existing == null) return null;

            existing.Name = menu.Name;
            existing.Price = menu.Price;
            existing.ImageUrl = menu.ImageUrl;
            existing.Cost = menu.Cost;
            existing.Active = menu.Active;
            existing.IsTopping = menu.IsTopping;
            existing.CategoryId = menu.CategoryId;

            // ตรวจสอบว่า IdInFirestore ไม่ว่างเปล่า
            if (!string.IsNullOrEmpty(menu.MenuIdInFirestore))// ตรวจสอบว่า IdInFirestore ไม่ว่างเปล่า
            {
                existing.MenuIdInFirestore = menu.MenuIdInFirestore;// อัปเดต IdInFirestore ด้วยค่าใหม่
            }

            _context.Menus.Update(existing);
            _context.SaveChanges();
            return existing;
        }

        public bool DeleteMenu(int id)
        {
            var menu = _context.Menus.Find(id);
            if (menu == null) return false;

            _context.Menus.Remove(menu);
            _context.SaveChanges();
            return true;
        }

        public async Task<string> CopyMenusFromFirestore()
        {
            try
            {
                //ดึงข้อมูลจาก Firestore
                //var snapshot = await _utilService.GetSnapshotFromFirestoreByCollectionNameAndOrderBy("menu", "category"); // ดึงข้อมูลจาก collection "menu" และเรียงตาม "category"
                var snapshot = await _firestoreService.GetMenusAsync(); // ดึงข้อมูลจาก collection "menu" และเรียงตาม "category"
                if (snapshot.Documents.Count == 0)
                {
                    return "ไม่มีเมนูใน Firestore ที่จะคัดลอก";
                }
                int copied = 0;
                int UpdateValue = 0;
                // 2. Map ข้อมูลหลักจาก Firestore
                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();

                    // 1. Map ข้อมูลหลักจาก Firestore
                    var categoryName = data["category"]?.ToString() ?? "";
                    Category _Category = _context.Categories.First(x => x.CategoryInFirestore == categoryName);

                    var existingOrder = await _context.Menus.FirstOrDefaultAsync(m => m.MenuIdInFirestore == doc.Id);

                    if (existingOrder != null)
                    {
                        existingOrder.Name = data["name"]?.ToString() ?? "";
                        existingOrder.Price = Convert.ToDecimal(data["price"]);
                        existingOrder.Cost = Convert.ToDecimal(data["cost"]);
                        existingOrder.ImageUrl = data["imgPath"]?.ToString() ?? "";
                        existingOrder.Active = Convert.ToBoolean(data["active"]);
                        existingOrder.IsTopping = Convert.ToBoolean(data["addTopping"]);
                        existingOrder.CategoryId = _Category.CategoryId;
                        existingOrder.Category = _Category;
                        await _context.SaveChangesAsync();
                        UpdateValue++;
                    }
                    else
                    {
                        var menu = new Menu
                        {
                            MenuIdInFirestore = doc.Id, // ใช้ ID ของ Firestore
                            Name = data["name"]?.ToString() ?? "",
                            Price = Convert.ToDecimal(data["price"]),
                            Cost = Convert.ToDecimal(data["cost"]),
                            ImageUrl = data["imgPath"]?.ToString() ?? "",
                            Active = Convert.ToBoolean(data["active"]),
                            IsTopping = Convert.ToBoolean(data["addTopping"]),
                            CategoryId = _Category.CategoryId,
                            Category = _Category
                        };
                        _context.Menus.Add(menu);
                        await _context.SaveChangesAsync();
                        copied++;
                        Console.WriteLine($"คัดลอกเมนู: {menu.Name} (ID: {menu.MenuIdInFirestore})");
                    }
                }

                return $"คัดลอก เมนู จาก Firestore มา {copied} รายการ และ อัพเดทเมนู {UpdateValue} รายการเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกเมนูจาก Firestore");
                return "เกิดข้อผิดพลาดในการคัดลอกเมนู";
            }
        }

        public async Task<Menu> CopyMenusFromFirestoreByID(string firebaseMenuID)
        {
            try
            {
                var snapshot = await _utilService.GetSnapshotFromFirestoreWithID("menu", firebaseMenuID);
            if (snapshot == null || snapshot.Documents.Count == 0) return new Menu();

            var doc = snapshot.Documents.First();
            var data = doc.ToDictionary();
            var categoryName = data.ContainsKey("category") ? data["category"]?.ToString() ?? "" : "";
            var _Category = await _context.Categories.FirstOrDefaultAsync(x => x.CategoryInFirestore == categoryName);
            if (_Category == null) return new Menu();

            // ตรวจสอบว่ามีเมนูนี้ในฐานข้อมูลหรือยัง
            var existingMenu = await _context.Menus.FirstOrDefaultAsync(m => m.MenuIdInFirestore == doc.Id);
            if (existingMenu != null)
            {
                // ถ้ามีอยู่แล้ว ให้ return เมนูนั้นกลับไป
                return existingMenu;
            }

            var menu = new Menu
            {
                MenuIdInFirestore = doc.Id,
                Name = data.ContainsKey("name") ? data["name"]?.ToString() ?? "" : "",
                Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"]) : 0,
                Cost = data.ContainsKey("cost") ? Convert.ToDecimal(data["cost"]) : 0,
                ImageUrl = data.ContainsKey("imgPath") ? data["imgPath"]?.ToString() ?? "" : "",
                Active = data.ContainsKey("active") ? Convert.ToBoolean(data["active"]) : false,
                IsTopping = data.ContainsKey("addTopping") ? Convert.ToBoolean(data["addTopping"]) : false,
                CategoryId = _Category.CategoryId,
                Category = _Category
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return menu;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกเมนูจาก Firestore");
                return new Menu();
            }
        }
    }
}