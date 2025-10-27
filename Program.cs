using chickko.api.Data;
using Microsoft.EntityFrameworkCore;
using chickko.api.Services;
using chickko.api.Interface;
using System.Globalization;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // ⭐️ Enable Controllers API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// เพิ่มเพื่ออ่าน HttpContext
builder.Services.AddHttpContextAccessor();

// เพิ่ม SiteService
builder.Services.AddScoped<ISiteService, SiteService>();

// เปลี่ยน AddDbContext: ใช้ connection string ตาม Site (แทน DefaultConnection เดิม)
builder.Services.AddDbContext<ChickkoContext>((sp, options) =>
{
    var siteService = sp.GetRequiredService<ISiteService>();
    options.UseNpgsql(siteService.GetConnectionString());
});

builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IUtilService, UtilService>(); // เพิ่ม IUtilService
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ICostService, CostService>();
builder.Services.AddScoped<IWorktimeService, WorktimeService>();
builder.Services.AddScoped<IEventRollingService, chickko.api.Services.Event.EventRollingService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>(); // เพิ่ม IDateTimeService

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Web", policy =>
        policy
            .WithMethods("GET","POST","PUT","DELETE","PATCH","OPTIONS")
            .WithHeaders("*")
            .SetIsOriginAllowed(origin =>
            {
                try
                {
                    var host = new Uri(origin).Host;
                    return
                        host.EndsWith(".vercel.app") ||                 // vercel preview ทั้งหมด
                        origin == "https://chickko-pos.vercel.app" ||   // vercel prod
                        origin == "https://chickkoapp.web.app" ||     // firebase (ถ้ามี)
                        origin.StartsWith("http://localhost:") ||     // dev http (vite ส่วนใหญ่ 5173/4173)
                        origin.StartsWith("https://localhost:") ||     // dev https (เผื่อมี)
                        origin.StartsWith("http://127.0.0.1:") ||
                        origin.StartsWith("https://127.0.0.1:") ||
                        origin.StartsWith("http://localhost:5173");
                }
                catch { return false; }
            })
            .DisallowCredentials()  // ใช้ Bearer header, ไม่ต้องเปิด credentials
    );
});

// Set default culture
// Set default culture
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");


var app = builder.Build();

app.UseCors("Web");  
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.MapControllers(); // ⭐️ Map Controller routes

// Minimal API ที่มีอยู่แล้ว
app.MapGet("/menus", async (ChickkoContext db) =>
    await db.Menus.ToListAsync());

// Debug endpoint สำหรับตรวจสอบ environment variables
app.MapGet("/debug/env", () =>
{
    var envVars = new Dictionary<string, object>();
    
    // ตรวจสอบ environment variables ที่เกี่ยวข้อง
    var credentialsExists = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON_HKT"));
    var credentialsLength = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON_HKT")?.Length ?? 0;
    
    envVars.Add("GOOGLE_APPLICATION_CREDENTIALS_JSON_EXISTS", credentialsExists);
    envVars.Add("GOOGLE_APPLICATION_CREDENTIALS_JSON_LENGTH", credentialsLength);
    envVars.Add("ENVIRONMENT", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown");
    envVars.Add("TEMP_PATH", Path.GetTempPath());
    
    return Results.Ok(envVars);
});

// Health check endpoint สำหรับตรวจสอบ Firebase connection
app.MapGet("/health/firebase", async (IUtilService utilService) =>
{
    try
    {
        // ลองดึงข้อมูลจาก Firestore collection ใดก็ได้
        var snapshot = await utilService.GetSnapshotFromFirestoreByCollectionName("orders");
        return Results.Ok(new { 
            status = "healthy",
            message = "Firebase connection successful",
            timestamp = DateTime.UtcNow,
            documentsFound = snapshot.Documents.Count
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Firebase connection failed",
            instance: "/health/firebase"
        );
    }
});

app.Run();

// ✅ แทนที่ DateTimeService เดิม
public class DateTimeService : IDateTimeService
{
    // ✅ สร้าง Custom TimeZone UTC+7 เอง
    private static readonly TimeZoneInfo BangkokTimeZone = TimeZoneInfo.CreateCustomTimeZone(
        "Bangkok Standard Time",
        TimeSpan.FromHours(7),  // ✅ ชัดเจน UTC+7
        "Bangkok Standard Time",
        "BST"
    );

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BangkokTimeZone);
    public DateOnly Today => DateOnly.FromDateTime(Now);
    public TimeOnly TimeNow => TimeOnly.FromDateTime(Now);
}

public interface IDateTimeService
{
    DateTime Now { get; }
    DateOnly Today { get; }
    TimeOnly TimeNow { get; }
}