using chickko.api.Data;
using chickko.api.Models;

namespace chickko.api.Services
{
    public class MenuService : IMenuService
    {
        private readonly ChickkoContext _context;

        public MenuService(ChickkoContext context)
        {
            _context = context;
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
    }
}