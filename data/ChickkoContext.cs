using Microsoft.EntityFrameworkCore;
using chickko.api.Models;
using static OrdersService;

namespace chickko.api.Data
{
    public class ChickkoContext : DbContext
    {
        public ChickkoContext(DbContextOptions<ChickkoContext> options) : base(options)
        {
        }

        public DbSet<Menu> Menus { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserPermistion> UserPermistion { get; set; } = null!;
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Ordertype> Ordertypes { get; set; }
        public DbSet<DischargeType> DischargeTypes { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Stock> Stocks { get; set; } = null!;
        public DbSet<StockLog> StockLogs { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Table> Tables { get; set; } = null!;
        public DbSet<OrderDetailTopping> OrderDetailToppings { get; set; } = null!;
        public DbSet<ImportOrderExcel> ImportOrdersExcel { get; set; } = null!;
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Cost> Cost { get; set; } = null!;
        public DbSet<CostCategory> CostCategory { get; set; } = null!;
        public DbSet<Worktime> Worktime { get; set; } = null!;
        public DbSet<Stock> Stock { get; set; } = null!;
        public DbSet<StockLog> StockLog { get; set; } = null!;
        public DbSet<Supplier> Supplier { get; set; } = null!;
        public DbSet<StockCategory> StockCategory { get; set; } = null!;
        public DbSet<StockUnitType> StockUnitType { get; set; } = null!;
        public DbSet<StockLocation> StockLocation { get; set; } = null!;
        public DbSet<StockLogType> StockLogType { get; set; } = null!;
        public DbSet<StockUnitCostHistory> StockUnitCostHistory { get; set; } = null!;
        public DbSet<Delivery> Deliveries { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChickkoContext).Assembly);
            // OrderHeader ↔ OrderDetail (1:N)
            modelBuilder.Entity<OrderHeader>()
                .HasMany<OrderDetail>() // ใช้ HasMany เพื่อระบุความสัมพันธ์ 1:N
                .WithOne(od => od.OrderHeader) // ใช้ WithOne เพื่อระบุความสัมพันธ์ N:1
                .HasForeignKey(od => od.OrderId) // Foreign Key
                .OnDelete(DeleteBehavior.Cascade); // ลบ order แล้วลบ detail ด้วย

            // OrderDetail ↔ OrderDetailTopping (1:N)
            modelBuilder.Entity<OrderDetail>()
                .HasMany(od => od.Toppings) // ใช้ HasMany เพื่อระบุความสัมพันธ์ 1:N
                .WithOne(t => t.OrderDetail) // ใช้ WithOne เพื่อระบุความสัมพันธ์ N:1
                .HasForeignKey(t => t.OrderDetailId) // Foreign Key
                .OnDelete(DeleteBehavior.Cascade); // ลบ detail แล้วลบ topping ด้วย

            // OrderDetailTopping → Menu (N:1) [ท้อปปิ้ง]
            modelBuilder.Entity<OrderDetailTopping>()
                .HasOne(t => t.Menu)
                .WithMany()
                .HasForeignKey(t => t.MenuId)
                .OnDelete(DeleteBehavior.Restrict); // ไม่ให้ลบเมนูแล้วกระทบ topping

            // OrderDetail → Menu (N:1)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Menu)
                .WithMany()
                .HasForeignKey(od => od.MenuId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderHeader → OrderType
            modelBuilder.Entity<OrderHeader>()
                .HasOne(o => o.OrderType)
                .WithMany()
                .HasForeignKey(o => o.OrderTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderHeader → DischargeType
            modelBuilder.Entity<OrderHeader>()
                .HasOne(o => o.DischargeType)
                .WithMany()
                .HasForeignKey(o => o.DischargeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderHeader → Discount (nullable)
            modelBuilder.Entity<OrderHeader>()
                .HasOne(o => o.Discount)
                .WithMany()
                .HasForeignKey(o => o.DiscountID)
                .OnDelete(DeleteBehavior.SetNull);

            // OrderHeader → Table (nullable)
            modelBuilder.Entity<OrderHeader>()
                .HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderHeader>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.OrderId)
                    .ValueGeneratedOnAdd();  // บอก EF ว่าค่านี้ auto generate
            });

            modelBuilder.Entity<ImportOrderExcel>(entity =>
            {
                entity.HasNoKey();          // เนื่องจาก table นี้ไม่มี Primary Key
                entity.ToView(null);        // เพื่อไม่ให้ EF คิดว่าเป็น View จริง
            });
        }

    }
}
