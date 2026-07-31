using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace chickko.api.Data
{
    public class ChickkoContextFactory : IDesignTimeDbContextFactory<ChickkoContext>
    {
        public ChickkoContext CreateDbContext(string[] args)
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional:false)
                .AddEnvironmentVariables()
                .Build();

            // ใช้ SITE=BKK เพื่อสลับฐาน (ถ้าไม่ตั้ง จะใช้ DefaultSite -> HKT)
            var site = Environment.GetEnvironmentVariable("SITE") ?? cfg["DefaultSite"] ?? "HKT";
            var conn = cfg.GetConnectionString(site)
                       ?? throw new InvalidOperationException($"Missing connection for site {site}.");

            var opt = new DbContextOptionsBuilder<ChickkoContext>()
                .UseNpgsql(conn)
                .Options;

            return new ChickkoContext(opt);
        }
    }
}