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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://chickkoapp.web.app", // ✅ เพิ่ม origin ของ frontend ตัวจริง
                "https://chickkoapi.up.railway.app",
                "http://localhost:5501",
                "http://127.0.0.1:5501"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
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
app.UseCors("AllowAll");
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