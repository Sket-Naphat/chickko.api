using chickko.api.Models;

namespace chickko.api.Services
{
    public interface IMenuService
    {
        List<Menu> GetMenus();
        Menu? GetMenu(int id);
        Menu CreateMenu(Menu menu);
        Menu? UpdateMenu(int id, Menu menu);
        bool DeleteMenu(int id);
        Task<string> CopyMenusFromFirestore();
        Task<Menu> CopyMenusFromFirestoreByID(string firebaseMenuID);
    }
}