using Microsoft.EntityFrameworkCore;
using chickko.api.Models;

namespace chickko.api.Data
{
    public class ChickkoContext : DbContext
    {
        public ChickkoContext(DbContextOptions<ChickkoContext> options) : base(options)
        {
        }

        public DbSet<Menu> Menus { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}