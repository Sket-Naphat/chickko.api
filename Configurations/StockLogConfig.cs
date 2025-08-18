using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using chickko.api.Models;

namespace chickko.api.Configurations
{
    public class StockLogConfig : IEntityTypeConfiguration<StockLog>
    {
        public void Configure(EntityTypeBuilder<StockLog> entity)
        {
            entity.HasOne(e => e.Supplier)
                  .WithMany()                      // หรือ .WithMany(s => s.StockLogs) ถ้ามี collection
                  .HasForeignKey(e => e.SupplyID)  // ✅ ชี้ว่า FK คือ SupplyID เท่านั้น
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_StockLog_Supplier_SupplyID");

            // กัน EF เผลอเดา shadow FK ชื่อ SupplierSupplyID
            entity.Ignore("SupplierSupplyID");

            entity.HasIndex(e => e.SupplyID);
        }
    }
}
