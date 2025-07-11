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
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<LocationOrder> LocationOrders { get; set; }
        public DbSet<Ordertype> Ordertypes { get; set; }
        public DbSet<DischargeType> DischargeTypes { get; set; }
        public DbSet<Discount> Discounts { get; set; }

    }
}