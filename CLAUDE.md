# chickko.api

## Project Name & Purpose
Backend REST API สำหรับระบบ POS (Point of Sale) ร้านไก่ ChickKo — จัดการออเดอร์, เมนู, ต้นทุน, สต็อก, พนักงาน, รายงานรายวัน และ Event

## Tech Stack & Major Dependencies
- **Runtime:** .NET 8 / ASP.NET Core Web API
- **ORM:** Entity Framework Core 9 + Npgsql (PostgreSQL)
- **Auth:** JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer 8.0)
- **Firebase:** Google.Cloud.Firestore 3.10 (ดึงออเดอร์จาก Firestore), Google.Apis.Auth
- **API Docs:** Swagger (Swashbuckle 6.6)
- **Container:** Docker (multi-stage build, dotnet/aspnet:8.0)

## Multi-Site Architecture
ระบบรองรับ 2 สาขา: **HKT** และ **BKK**
- `SiteService` เลือก connection string โดยอ่านจาก JWT claim `"Site"` → header `X-Site` → 던ข้อผิดพลาด
- `ChickkoContext` ถูก inject connection string ผ่าน `SiteService` ตอน request เข้า
- Connection strings อยู่ใน `appsettings.json` section `ConnectionStrings`

## Project Structure
```
chickko.api/
├── Controllers/         # HTTP Controllers (AuthController, OrdersController, CostController, StockController, StatementController, WorktimeController, EventController, MenuController)
├── Services/            # Business logic (implements Interface/)
│   └── Event/           # EventRollingService
├── Interface/           # Service interfaces
│   └── IEvent/
├── Models/              # EF Core entity models
│   └── Event/           # Rolling game models
├── Dtos/                # Data Transfer Objects
├── data/                # ChickkoContext + ChickkoContextFactory
├── Migrations/          # EF Core migrations (PostgreSQL)
├── Configurations/      # EF fluent config (StockLogConfig)
├── Middleware/          # (empty / reserved)
├── Firebase/            # credentials.json สำหรับ Google Firestore
├── Properties/          # launchSettings.json
├── Program.cs           # Entry point & DI setup
├── appsettings.json     # Connection strings, JWT config
└── Dockerfile           # Multi-stage Docker build
```

## Key Domain Modules
| Module | Controller | Service | Description |
|--------|-----------|---------|-------------|
| Auth | AuthController | AuthService | Login, JWT generation, LoginLog |
| Orders | OrdersController | OrdersService | Order CRUD, daily sales report (dine-in + delivery) |
| Menu | MenuController | MenuService | Menu management |
| Cost | CostController | CostService | ต้นทุน, CostCategory, CostPurchaseType |
| Stock | StockController | StockService | สต็อกวัตถุดิบ, StockLog, StockUnitCostHistory |
| Worktime | WorktimeController | WorktimeService | บันทึกเวลาทำงาน/ค่าจ้างพนักงาน |
| Statement | StatementController | StatementService | รายงานรายวัน (Balance sheet), Income, รายจ่ายคงค้าง |
| Event | EventController | EventRollingService | Event rolling game rewards |
| Site | — | SiteService | Multi-site routing |
| Util | — | UtilService | Helper (Thailand timezone, Firestore snapshot) |

## Entry Points
- **`Program.cs`** — เริ่มต้น app, register DI, middleware pipeline
- **`data/ChickkoContext.cs`** — EF DbContext, model relationships
- **`data/ChickkoContextFactory`** — สำหรับ `dotnet ef` CLI

## Key Conventions
- Controller routes: `api/[controller]`
- ทุก controller ใช้ `[Authorize]` (JWT)
- Services รับ `ChickkoContext`, `ILogger`, `IUtilService` ผ่าน constructor DI
- วันที่ใช้ `DateOnly`, เวลาใช้ `TimeOnly` (Thailand timezone ผ่าน `IUtilService`)
- `IsPurchase` flag ใน Cost/Worktime = true เมื่อจ่ายจริงแล้ว, false = รายจ่ายคงค้าง
- CostPurchaseType: 1=โอนธนาคาร, 2=เงินสดจ่าย, 3=เงินสดออก (สำหรับ Statement)
- CORS อนุญาต: `*.vercel.app`, `chickko-pos.vercel.app`, `localhost:*`

## Common Commands
```bash
# Run locally
dotnet run

# Build
dotnet build

# EF Migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Docker build
docker build -t chickko.api .
docker run -p 8080:8080 chickko.api
```

## Notable TODOs / Issues
- `appsettings.json` มี credential จริง (DB password, JWT key) — ไม่ควร commit ใน production
- Firebase credentials อยู่ใน `Firebase/credentials.json` — ควรใช้ env var แทน
- `data/ChickkoContext.cs` มี DbSet ซ้ำ (`Stock`, `StockLog`) ซึ่งทำงานได้ แต่ควรทำความสะอาด
- `Middleware/` โฟลเดอร์ยังว่างอยู่
- Debug endpoint `/debug/env` ควรลบออกใน production
