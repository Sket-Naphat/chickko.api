using chickko.api.Data;
using chickko.api.Interface;
using chickko.api.Models;

namespace chickko.api.Services
{
    public class MenuService : IMenuService
    {
        private readonly ChickkoContext _context;
        private readonly ILogger<MenuService> _logger;
        private readonly IUtilService _utilService;

        public MenuService(ChickkoContext context, ILogger<MenuService> logger, IUtilService utilService)
        {
            _context = context;
            _logger = logger;
            _utilService = utilService;
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
                var snapshot = await _utilService.GetSnapshotFromFirestoreByCollectionNameAndOrderBy("menu","category"); // ดึงข้อมูลจาก collection "menu" และเรียงตาม "category"
                if (snapshot.Documents.Count == 0)
                {
                    return "ไม่มีเมนูใน Firestore ที่จะคัดลอก";
                }
                int copied = 0;

                // 2. Map ข้อมูลหลักจาก Firestore
                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();

                    // 1. Map ข้อมูลหลักจาก Firestore
                    var categoryName = data["category"]?.ToString() ?? "";
                    int categoryId = _context.Categories.First(x => x.CategoryInFirestore == categoryName).CategoryId;

                    var menu = new Menu
                    {
                        MenuIdInFirestore = doc.Id, // ใช้ ID ของ Firestore
                        Name = data["name"]?.ToString() ?? "",
                        Price = Convert.ToDecimal(data["price"]),
                        Cost = Convert.ToDecimal(data["cost"]),
                        ImageUrl = data["imgPath"]?.ToString() ?? "",
                        Active = Convert.ToBoolean(data["active"]),
                        IsTopping = Convert.ToBoolean(data["addTopping"]),
                        CategoryId = categoryId
                    };

                    // 3. บันทึก OrderHeader ก่อน เพื่อให้ได้ OrderId
                    _context.Menus.Add(menu);
                    await _context.SaveChangesAsync();
                    copied++;
                    Console.WriteLine($"คัดลอกเมนู: {menu.Name} (ID: {menu.MenuIdInFirestore})");
                }
                
                return $"คัดลอก เมนู จาก Firestore มา {copied} รายการเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกเมนูจาก Firestore");
                return "เกิดข้อผิดพลาดในการคัดลอกเมนู";
            }
        }
    }
}