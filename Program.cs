using chickko.api.Data;
using Microsoft.EntityFrameworkCore;
using chickko.api.Services;
using chickko.api.Interface;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // ⭐️ Enable Controllers API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ChickkoContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IUtilService, UtilService>(); // เพิ่ม IUtilService
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ICostService, CostService>();
builder.Services.AddScoped<IWorktimeService, WorktimeService>();

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
                        origin == "https://chickkoapp.web.app"   ||     // firebase (ถ้ามี)
                        origin.StartsWith("http://localhost:")   ||     // dev http (vite ส่วนใหญ่ 5173/4173)
                        origin.StartsWith("https://localhost:")  ||     // dev https (เผื่อมี)
                        origin.StartsWith("http://127.0.0.1:")   ||
                        origin.StartsWith("https://127.0.0.1:");
                }
                catch { return false; }
            })
            .DisallowCredentials()  // ใช้ Bearer header, ไม่ต้องเปิด credentials
    );
});

var credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS_JSON");

if (!string.IsNullOrEmpty(credentialsJson))
{
    var filePath = Path.Combine(Path.GetTempPath(), "gcp-credentials.json");
    File.WriteAllText(filePath, credentialsJson);
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
}
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

app.Run();