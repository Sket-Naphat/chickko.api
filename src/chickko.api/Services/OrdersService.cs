using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using chickko.api.Data;
using chickko.api.Dtos;
using chickko.api.Interface;
using chickko.api.Models;
using chickko.api.Services;
using Google.Api;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
public class OrdersService : IOrdersService
{
    private readonly ChickkoContext _context;
    private readonly ILogger<OrdersService> _logger;
    private readonly IMenuService _menuService;

    private readonly IUtilService _utilService;

    public OrdersService(
        ChickkoContext context,
        ILogger<OrdersService> logger,
        IMenuService menuService,
        IUtilService utilService)
    {
        _context = context;
        _logger = logger;
        _menuService = menuService;
        _utilService = utilService;
    }

    // ✅ คุณต้องเขียนเองให้เชื่อมกับ Firestore SDK
    // และทำการคัดลอกข้อมูลจาก Firestore มายังฐานข้อมูล ChickkoContext
    public async Task<string> CopyOrderFromFirestore()
    {
        int copied = 0;
        try
        {
            //await InitializeFirestore();
            await _menuService.CopyMenusFromFirestore();

            var lastOrderDate = await _context.OrderHeaders.MaxAsync(o => o.OrderDate);
            var lastOrderDateString = lastOrderDate?.ToString("yyyy-MM-dd") ?? "";
            //var snapshot = await _firestoreService.GetOrdersAsync(lastOrderDateString, "");
            var snapshot = await _utilService.GetSnapshotFromFirestoreWithDateGreaterThan(
                    collectionName: "orders",
                    orderByField: "orderDate",
                    whereField: "orderDate",
                    dateTo: lastOrderDateString
                );


            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                // var whereorderDate = DateOnly.Parse(data["orderDate"]?.ToString() ?? "");
                // var whereorderTime = TimeOnly.Parse(data["orderTime"]?.ToString() ?? "");
                // var wherecustomerName = data["customerName"]?.ToString();

                // var existingOrder = await _context.OrderHeaders
                //     .FirstOrDefaultAsync(o =>
                //         o.OrderDate == whereorderDate &&
                //         o.CustomerName == wherecustomerName &&
                //         o.OrderTime.ToString() == whereorderTime.ToString("hh:mm:ss")
                //     );

                //เปลี่ยนมาใช้ query ตรงเนื่องจาก where ด้วย TimeOnly ไม่ได้สักที

                var _orderDate = DateOnly.Parse(data["orderDate"]?.ToString() ?? "").ToString("yyyy-MM-dd");
                var _orderTime = TimeOnly.Parse(data["orderTime"]?.ToString() ?? "").ToString("HH:mm:ss");
                var _customerName = data["customerName"]?.ToString()?.Replace("'", "''"); // escape ' ด้วย

                // var sql = $@"
                //     SELECT * FROM ""OrderHeaders"" 
                //     WHERE ""OrderDate"" = '{_orderDate}' 
                //     AND ""OrderTime"" = '{_orderTime}' 
                //     AND ""CustomerName"" = '{_customerName}'";

                // var existingOrder = await _context.OrderHeaders
                //     .FromSqlRaw(sql)
                //     .FirstOrDefaultAsync();
                var existingOrder = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.IdInFirestore == doc.Id);

                if (existingOrder != null)
                {
                    _logger.LogInformation($"Order already exists for {data["customerName"]} on {data["orderDate"]}. Skipping.");
                    continue; // ถ้าเจอแล้วข้ามไป
                }
                else
                {
                    //string dischargeName = (data["dischargeType"]?.ToString() ?? "Promptpay").Trim();
                    string locName = (data["locationOrder"]?.ToString() ?? "forHere").Trim();
                    string tableNumber = (data["tableNumber"]?.ToString() ?? "tw").Trim();

                    string dischargeName = "Promptpay"; // fallback default

                    if (data.TryGetValue("dischargeType", out var dischargeRaw) && dischargeRaw != null && (dischargeRaw as string) != "")
                    {
                        dischargeName = dischargeRaw.ToString()!.Trim();
                    }


                    Ordertype _OrderType = _context.Ordertypes.FirstOrDefault(x => x.OrderTypeName == locName) ?? new Ordertype();
                    DischargeType _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == dischargeName) ?? new DischargeType();
                    Table _Table = _context.Tables.FirstOrDefault(x => x.TableName == tableNumber) ?? new Table() { TableName = tableNumber };

                    var order = new OrderHeader
                    {
                        CustomerName = data["customerName"]?.ToString() ?? "ไม่ระบุชื้อ : " + data["orderDate"]?.ToString(),
                        OrderDate = DateOnly.TryParseExact(data["orderDate"]?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var orderDate) ? orderDate : DateOnly.FromDateTime(DateTime.Now),
                        OrderTime = TimeOnly.TryParse(data["orderTime"]?.ToString(), out var orderTime) ? orderTime : TimeOnly.FromDateTime(DateTime.Now),
                        OrderTypeId = _OrderType?.OrderTypeId ?? 0,
                        OrderType = _OrderType ?? new Ordertype(),
                        DischargeTypeId = _DischargeType?.DischargeTypeId ?? 0,
                        DischargeType = _DischargeType ?? new DischargeType(),
                        DischargeTime = TimeOnly.TryParse(data["dischargeTime"]?.ToString(), out var dTime) ? dTime : null,
                        IsDischarge = data.TryGetValue("discharge", out var disVal) && bool.TryParse(disVal?.ToString(), out var isDischargeParsed) ? isDischargeParsed : false,
                        FinishOrderTime = TimeOnly.TryParse(data["finishedOrderTime"]?.ToString(), out var fTime) ? fTime : null,
                        IsFinishOrder = Convert.ToBoolean(data["finishedOrder"]),
                        TotalPrice = 0,
                        OrderRemark = data["remark"]?.ToString() ?? "",
                        DiscountID = null,
                        Discount = null,
                        IdInFirestore = doc.Id,
                        TableID = _Table?.TableID ?? 0,
                        Table = _Table ?? new Table() { TableName = tableNumber },
                        ItemQTY = 0
                    };

                    var items = data["items"] as IEnumerable<object>; // ตรวจสอบว่า items มีข้อมูลหรือไม่
                    if (items == null) continue;

                    foreach (var itemObj in items)
                    {
                        var item = itemObj as Dictionary<string, object>;
                        if (item == null) continue;

                        string itemName = item["name"]?.ToString() ?? "";
                        string firebaseID = item["id"]?.ToString() ?? "";
                        //var parts = itemName.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var parts = firebaseID.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var baseItemID = parts.FirstOrDefault();
                        if (baseItemID == null) continue;

                        var menu = _context.Menus.FirstOrDefault(x => x.MenuIdInFirestore == baseItemID);
                        if (menu == null)
                        {
                            // _logger.LogWarning($"ไม่พบเมนูหลักชื่อ '{baseItemID}' ในฐานข้อมูล");
                            // continue;
                            menu = await _menuService.CopyMenusFromFirestoreByID(baseItemID);
                            if (menu == null)
                            {
                                _logger.LogWarning($"ไม่พบเมนูหลักชื่อ '{baseItemID}' ในฐานข้อมูล และไม่สามารถคัดลอกได้");
                                continue;
                            }
                        }
                        _context.Attach(menu); // ป้องกัน EF เพิ่มซ้ำ

                        bool _isDone = item.TryGetValue("done", out var doneVal) && bool.TryParse(doneVal?.ToString(), out var isDoneVal) ? isDoneVal : false;
                        bool _isDischarge = item.TryGetValue("itemDischarge", out var dischargeVal) && bool.TryParse(dischargeVal?.ToString(), out var isDischargeVal) ? isDischargeVal : false;

                        // สร้าง OrderDetail
                        var detail = new OrderDetail
                        {
                            //OrderId = order.OrderId, // ต้องแน่ใจว่า OrderId ถูกกำหนดก่อน
                            OrderHeader = order,
                            MenuId = menu.Id,
                            Menu = menu,
                            Quantity = Convert.ToInt32(item["quantity"]),
                            Price = menu.Price,
                            Toppings = new List<OrderDetailTopping>(),
                            MenuIdInFirestore = item["id"]?.ToString() ?? "",
                            IsDone = _isDone,
                            IsDischarge = _isDischarge,
                            Remark = item["remark"]?.ToString() ?? "",
                            ToppingQTY = 0
                        };

                        var toppingNames = parts.Skip(1);
                        foreach (var tName in toppingNames)
                        {
                            var topping = _context.Menus.FirstOrDefault(x => x.MenuIdInFirestore == tName.Trim());
                            if (topping == null)
                            {
                                topping = await _menuService.CopyMenusFromFirestoreByID(tName.Trim());
                                if (topping == null)
                                {
                                    _logger.LogWarning($"ไม่พบเมนูหลักชื่อ '{tName.Trim()}' ในฐานข้อมูล และไม่สามารถคัดลอกได้");
                                    continue;
                                }
                            }


                            _context.Attach(topping);

                            detail.Toppings.Add(new OrderDetailTopping
                            {
                                MenuId = topping.Id,
                                Menu = topping,
                                ToppingPrice = topping.Price
                            });

                            // เพิ่มราคาท้อปปิ้ง
                            detail.Price += topping.Price;

                            detail.ToppingQTY += 1;
                        }

                        // คำนวณยอดรวมคำสั่งซื้อ
                        order.TotalPrice += detail.Price * detail.Quantity;

                        _context.OrderDetails.Add(detail);
                        order.ItemQTY += 1;
                    }

                    _context.OrderHeaders.Add(order);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ คัดลอก Order ID: {order.OrderId} | วันที่: {order.OrderDate}, เวลา: {order.OrderTime}");
                    copied++;
                }

            }
            string returnString = $"✅ คัดลอกคำสั่งซื้อจาก Firestore แล้วทั้งหมด {copied} รายการ";
            if (copied == 0)
            {
                returnString = $"✅ การคัดลอกคำสั่งซื้อจาก Firestore Update ล่าสุดอยู่แล้ว";
            }

            return returnString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ เกิดข้อผิดพลาดขณะคัดลอกคำสั่งซื้อ");
            if (ex.InnerException != null)
            {
                _logger.LogError("🔥 InnerException: " + ex.InnerException.Message);
                Console.WriteLine("🔥 InnerException: " + ex.InnerException.Message);
            }
            return "❌ เกิดข้อผิดพลาด กรุณาตรวจสอบ " + ex.Message + " เพิ่มเติม";
        }
    }

    public async Task<string> ImportOrderFromExcel()
    {
        int copied = 0;

        try
        {
            var importData = await _context.Set<ImportOrderExcel>()
                .FromSqlRaw("SELECT * FROM import_orders_excel ORDER BY order_date, order_time, customer_name")
                .ToListAsync();

            var groupedOrders = importData
                .GroupBy(d => new
                {
                    customer = d.customer_name?.Trim(),
                    date = d.order_date?.Trim(),
                    time = string.IsNullOrWhiteSpace(d.order_time) ? "17:00:00" : d.order_time.Trim()
                });

            foreach (var group in groupedOrders)
            {
                var key = group.Key;

                if (string.IsNullOrWhiteSpace(key.customer) || string.IsNullOrWhiteSpace(key.date))
                    continue;


                if (!DateOnly.TryParseExact(key.date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    _logger.LogWarning($"❌ วันที่ไม่ถูกต้อง: {key.date}");
                    continue;
                }

                if (!TimeOnly.TryParse(key.time, out var parsedTime))
                {
                    _logger.LogWarning($"❌ เวลาไม่ถูกต้อง: {key.time}");
                    continue;
                }

                var existingOrder = await _context.OrderHeaders.FirstOrDefaultAsync(o =>
                    o.CustomerName == key.customer &&
                    o.OrderDate == parsedDate &&
                    o.OrderTime == parsedTime);

                if (existingOrder != null)
                {
                    _logger.LogInformation($"⚠️ ซ้ำ: {key.customer} {key.date} {key.time}");
                    continue;
                }

                var first = group.First();
                if (string.IsNullOrEmpty(first.discharge_type))
                {
                    first.discharge_type = "ทานที่ร้าน";
                }

                Ordertype _OrderType = _context.Ordertypes.FirstOrDefault(x => x.Description == first.discharge_type) ?? new Ordertype(); // ตรวจสอบว่า OrderType มีอยู่ในฐานข้อมูลหรือไม่
                DischargeType _DischargeType = new DischargeType();
                if (first.cash_price != "0")
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Promptpay") ?? new DischargeType { };
                }
                else if (first.promptpay_price != "0")
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Cash") ?? new DischargeType { };
                }
                else if (first.discharge_type == "grab")
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Grab") ?? new DischargeType { };
                }
                else
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Promptpay") ?? new DischargeType { };
                }
                Table _Table = _context.Tables.FirstOrDefault(x => x.TableName == "tw") ?? new Table(); // ตรวจสอบว่า Table มีอยู่ในฐานข้อมูลหรือไม่

                var orderHeader = new OrderHeader
                {
                    CustomerName = key.customer,
                    OrderDate = parsedDate,
                    OrderTime = parsedTime,
                    OrderTypeId = _OrderType?.OrderTypeId ?? 0,
                    OrderType = _OrderType ?? new Ordertype(), // ใช้ค่าเริ่มต้นถ้าไม่พบ

                    DischargeTypeId = _DischargeType.DischargeTypeId,
                    DischargeType = _DischargeType, // ใช้ค่าเริ่มต้นถ้าไม่พบ
                    DischargeTime = TimeOnly.TryParse(first.discharge_time, out var dTime) ? dTime : null,
                    // IsDischarge = Convert.ToBoolean(data["discharge"]),
                    IsDischarge = !string.IsNullOrWhiteSpace(first.discharge_time),
                    FinishOrderTime = TimeOnly.TryParse(first.finish_order_time, out var fTime) ? fTime : null,
                    IsFinishOrder = !string.IsNullOrWhiteSpace(first.finish_order_time),
                    TotalPrice = 0,
                    OrderRemark = "",
                    DiscountID = null,
                    Discount = null,
                    IdInFirestore = "",
                    TableID = _Table.TableID,
                    Table = _Table ?? new Table(),// ใช้ค่าเริ่มต้นถ้าไม่พบ
                    ItemQTY = 0
                };

                foreach (var item in group)
                {
                    if (string.IsNullOrWhiteSpace(item.menu_name)) continue;

                    var baseItem = await _context.Menus.FirstOrDefaultAsync(m => m.Name == item.menu_name.Trim());
                    if (baseItem == null)
                    {
                        _logger.LogWarning($"❌ เมนูไม่พบ: {item.menu_name}");
                        continue;
                    }
                    baseItem.Category = await _context.Categories.FirstOrDefaultAsync(m => m.CategoryId == baseItem.CategoryId);

                    var quantity = int.TryParse(item.qty, out var qtyVal) ? qtyVal : 1;
                    var price = decimal.TryParse(item.price, out var priceVal) ? priceVal : baseItem.Price;

                    var orderDetail = new OrderDetail
                    {
                        OrderHeader = orderHeader,
                        MenuId = baseItem.Id,
                        Quantity = quantity,
                        Price = price,
                        Menu = baseItem,
                        IsDone = true,
                        IsDischarge = true,
                        Remark = string.Empty,
                        MenuIdInFirestore = "",
                        Toppings = new List<OrderDetailTopping>()
                    };

                    orderHeader.TotalPrice += price;//* quantity;
                    _context.OrderDetails.Add(orderDetail);
                    orderHeader.ItemQTY += 1;
                }

                _context.OrderHeaders.Add(orderHeader);
                await _context.SaveChangesAsync();
                copied++;
            }

            return $"✅ คัดลอกคำสั่งซื้อแล้วทั้งหมด {copied} รายการ";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ เกิดข้อผิดพลาดระหว่าง import");
            if (ex.InnerException != null)
            {
                _logger.LogError("🔥 InnerException: " + ex.InnerException.Message);
            }

            return "❌ เกิดข้อผิดพลาด: " + ex.Message;
        }
    }
    public async Task InitializeFirestore()
    {
        try
        {
            var credPath = @"d:\Sket\sket_project\chickko.api\Firebase\credentials_bkk.json";
            var json = await File.ReadAllTextAsync(credPath);

            Console.WriteLine("📁 File loaded successfully");

            // Parse เพื่อดูข้อมูล
            using var doc = JsonDocument.Parse(json);
            var projectId = doc.RootElement.GetProperty("project_id").GetString();
            var clientEmail = doc.RootElement.GetProperty("client_email").GetString();

            Console.WriteLine($"Project: {projectId}");
            Console.WriteLine($"Email: {clientEmail}");

            // สร้าง credential
            var credential = GoogleCredential.FromJson(json).CreateScoped(FirestoreClient.DefaultScopes);
            Console.WriteLine("✅ Credential created");

            // ทดสอบ access token
            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            Console.WriteLine($"✅ Access token obtained: {token.Substring(0, 20)}...");

            // ทดสอบ Firestore connection
            var db = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = credential
            }.Build();

            Console.WriteLine("✅ Firestore DB created successfully");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"❌ Inner: {ex.InnerException.Message}");
        }
    }
    public async Task<List<DailySaleDto>> GetDailyDineInSalesReport(SaleDateDto saleDateDto)
    {
        try
        {
            // ✅ สร้าง query เพื่อกรองยอดขายหน้าร้าน
            var query = _context.OrderHeaders
                .Include(oh => oh.OrderType)
                .Include(oh => oh.DischargeType)
                .Where(oh => oh.OrderTypeId != 3);   // กรองยอดขายหน้าร้าน (ไม่ใช่เดลิเวอรี่)

            var orderDetailsQuery = _context.OrderDetails
                .Include(od => od.Menu)
                .Include(od => od.OrderHeader)
                .Where(od => od.OrderHeader != null && od.OrderHeader.OrderTypeId != 3);
            // && od.MenuId != 20 && od.MenuId != 7); // กรองยอดขายหน้าร้าน , != 20 คือ ไม่นับน้ำเปล่า 7 โค้ก

            // ✅ แก้ไขการกรองปี/เดือน - ใช้ .Year/.Month property ของ DateOnly
            if (saleDateDto.DateFrom.HasValue)
            {
                query = query.Where(c => c.OrderDate.HasValue && c.OrderDate.Value >= saleDateDto.DateFrom.Value);
                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue && od.OrderHeader.OrderDate.Value >= saleDateDto.DateFrom.Value);
            }
            if (saleDateDto.DateTo.HasValue)
            {
                query = query.Where(c => c.OrderDate.HasValue && c.OrderDate.Value <= saleDateDto.DateTo.Value);
                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue && od.OrderHeader.OrderDate.Value <= saleDateDto.DateTo.Value);
            }

            // เงื่อนไข year/month เดิมยังคงใช้ได้ (จะถูก where ซ้อนกัน)
            if (saleDateDto.Year.HasValue && saleDateDto.Month.HasValue)
            {
                query = query.Where(c => c.OrderDate.HasValue &&
                    c.OrderDate.Value.Year == saleDateDto.Year.Value &&
                    c.OrderDate.Value.Month == saleDateDto.Month.Value);

                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue &&
                    od.OrderHeader.OrderDate.Value.Year == saleDateDto.Year.Value &&
                    od.OrderHeader.OrderDate.Value.Month == saleDateDto.Month.Value);
            }
            else if (saleDateDto.Year.HasValue)
            {
                query = query.Where(c => c.OrderDate.HasValue &&
                    c.OrderDate.Value.Year == saleDateDto.Year.Value);

                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue &&
                    od.OrderHeader.OrderDate.Value.Year == saleDateDto.Year.Value);
            }
            else if (saleDateDto.Month.HasValue)
            {
                query = query.Where(c => c.OrderDate.HasValue &&
                    c.OrderDate.Value.Month == saleDateDto.Month.Value);

                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue &&
                    od.OrderHeader.OrderDate.Value.Month == saleDateDto.Month.Value);
            }

            // ✅ ดึงข้อมูล order headers สำหรับ daily sales และ peak hours
            var allOrderHeaders = await query.ToListAsync();

            // ✅ คำนวณ Peak Hours สำหรับแต่ละวัน - ทุกช่วงที่มีออเดอร์
            var dailyPeakHours = allOrderHeaders
                .Where(oh => oh.OrderDate.HasValue && oh.OrderTime.HasValue)
                .GroupBy(oh => oh.OrderDate!.Value)
                .ToDictionary(dateGroup => dateGroup.Key, dateGroup =>
                {
                    // หาช่วงเวลาที่มีออเดอร์
                    var ordersWithTime = dateGroup.Where(oh => oh.OrderTime.HasValue).ToList();

                    if (!ordersWithTime.Any())
                        return new List<PeakHourDto>();

                    // หาช่วงเวลาตั้งแต่ออเดอร์แรกถึงออเดอร์สุดท้าย
                    var firstOrderHour = ordersWithTime.Min(oh => oh.OrderTime!.Value.Hour);
                    var lastOrderHour = ordersWithTime.Max(oh => oh.OrderTime!.Value.Hour);

                    // สร้างรายการ PeakHour สำหรับทุกช่วงเวลาที่มีออเดอร์
                    var peakHours = new List<PeakHourDto>();

                    for (int hour = firstOrderHour; hour <= lastOrderHour; hour++)
                    {
                        var ordersInHour = ordersWithTime
                            .Where(oh => oh.OrderTime!.Value.Hour == hour)
                            .ToList();

                        // เฉพาะช่วงที่มีออเดอร์เท่านั้น
                        if (ordersInHour.Any())
                        {
                            // ✅ คำนวณยอดรวมในชั่วโมงนั้นจาก TotalPrice
                            var hourlyTotalSales = ordersInHour.Sum(oh => oh.TotalPrice);
                            var hourlyOrderCount = ordersInHour.Count;

                            peakHours.Add(new PeakHourDto
                            {
                                HourRange = $"{hour:D2}:00-{(hour + 1):D2}:00",
                                OrderCount = hourlyOrderCount,
                                TotalSales = hourlyTotalSales, // ✅ ยอดรวมจริงในชั่วโมงนั้น

                                // ✅ AvgPerOrder = ยอดรวมในชั่วโมง ÷ จำนวน order ในชั่วโมง
                                AvgPerOrder = Math.Round(
                                    (double)(hourlyOrderCount > 0 ? hourlyTotalSales / hourlyOrderCount : 0), 2)
                            });
                        }
                    }

                    // เรียงตามจำนวนออเดอร์มากที่สุด
                    return peakHours.OrderByDescending(x => x.OrderCount).ToList();
                });

            // ✅ ดึงข้อมูล daily sales โดยเอาแค่ AvgPerOrder
            var dailySales = allOrderHeaders
                .Where(oh => oh.OrderDate.HasValue)
                .GroupBy(oh => oh.OrderDate!.Value)
                .Select(g =>
                {
                    var totalAmount = g.Sum(x =>
                    {
                        var discountPrice = x.DiscountPrice ?? 0; // ถ้า null ให้เป็น 0
                        return discountPrice > 0
                            ? x.TotalPrice - discountPrice  // หัก discount ถ้ามี
                            : x.TotalPrice;                 // ไม่หัก discount ถ้าไม่มี
                    });
                    var orderCount = g.Count(); // ✅ จำนวน order ในวันนั้น

                    return new DailySaleDto
                    {
                        SaleDate = g.Key,
                        Orders = orderCount,
                        TotalAmount = totalAmount,
                        TotalDiscount = g.Sum(x => x.DiscountPrice ?? 0), // ยอดส่วนลดรวมในวันนั้น
                        // ✅ AvgPerOrder = ยอดรวมทั้งหมด ÷ จำนวน order
                        AvgPerOrder = Math.Round(
                            (double)(orderCount > 0 ? totalAmount / orderCount : 0), 2),

                        TopSellingItems = new List<SoldMenuDto>(),
                        totalOrders = orderCount,

                        // ✅ เพิ่ม PeakHours
                        PeakHours = new List<PeakHourDto>()
                    };
                })
                .OrderByDescending(x => x.SaleDate)
                .ToList();

            // ✅ ดึงข้อมูล order details ครั้งเดียวแล้วจัดกลุ่มใน memory สำหรับ TopSellingItems
            var allOrderDetails = await orderDetailsQuery.ToListAsync();

            // Group by date and menu, then calculate top selling items for each date
            var dailyMenuSales = allOrderDetails
                .Where(od => od.OrderHeader.OrderDate.HasValue)
                .GroupBy(od => od.OrderHeader.OrderDate!.Value)
                .ToDictionary(dateGroup => dateGroup.Key, dateGroup =>
                    dateGroup
                        .GroupBy(od => new { od.MenuId, MenuName = od.Menu?.Name ?? "Unknown" })
                        .Select(menuGroup => new SoldMenuDto
                        {
                            MenuId = menuGroup.Key.MenuId,
                            MenuName = menuGroup.Key.MenuName,
                            QuantitySold = menuGroup.Sum(od => od.Quantity),
                            TotalSales = menuGroup.Sum(od => od.Price * od.Quantity),
                            TotalCost = menuGroup.Sum(od => (od.Menu?.Cost ?? 0) * od.Quantity),
                            TotalProfit = menuGroup.Sum(od => (od.Price - (od.Menu?.Cost ?? 0)) * od.Quantity),
                            ProfitMargin = menuGroup.Sum(od => od.Price * od.Quantity) > 0 ?
                                (double)(menuGroup.Sum(od => (od.Price - (od.Menu?.Cost ?? 0)) * od.Quantity) /
                                menuGroup.Sum(od => od.Price * od.Quantity) * 100) : 0
                        })
                        .OrderByDescending(x => x.QuantitySold)
                        //.Take(5)
                        .ToList()
                );

            // ✅ Assign ข้อมูลเพิ่มเติมให้แต่ละ daily sale
            foreach (var dailySale in dailySales)
            {
                if (dailySale.SaleDate != null)
                {
                    // Assign top selling items
                    if (dailyMenuSales.TryGetValue(dailySale.SaleDate.Value, out var topItems))
                    {
                        dailySale.TopSellingItems = topItems;
                    }

                    // ✅ Assign peak hours (ทุกช่วงที่มีออเดอร์)
                    if (dailyPeakHours.TryGetValue(dailySale.SaleDate.Value, out var peakHours))
                    {
                        dailySale.PeakHours = peakHours;
                    }
                }
            }

            // ✅ เพิ่ม logging สำหรับ debug
            _logger.LogInformation($"📊 GetDailyDineInSalesReport: Found {dailySales.Count} records" +
                $" | Year: {saleDateDto.Year}" +
                $" | Month: {saleDateDto.Month}" +
                $" | Excluding MenuId: 20, 7");

            return dailySales;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ เกิดข้อผิดพลาดขณะดึงรายงานยอดขายหน้าร้าน");
            if (ex.InnerException != null)
            {
                _logger.LogError("🔥 InnerException: " + ex.InnerException.Message);
            }
            return new List<DailySaleDto>();
        }
    }
    public async Task<List<DailySaleDto>> GetDailyDeliverySalesReport(SaleDateDto saleDateDto)
    {
        try
        {
            // ✅ สร้าง query เพื่อกรองยอดขายเดลิเวอรี่จาก Deliveries
            var deliveryQuery = _context.Deliveries.AsQueryable();
            // ✅ ดึงข้อมูลจำนวน Orders จาก OrderHeaders (ในช่วงเวลาเดียวกัน)
            var orderCountQuery = _context.OrderHeaders.Where(oh => oh.OrderTypeId == 3); // เฉพาะเดลิเวอรี่

            // ✅ เพิ่ม query สำหรับ order details ของ delivery
            var orderDetailsQuery = _context.OrderDetails
                .Include(od => od.Menu)
                .Include(od => od.OrderHeader)
                .Where(od => od.OrderHeader != null && od.OrderHeader.OrderTypeId == 3); // เฉพาะเดลิเวอรี่ 

            // ✅ เพิ่ม filter DateFrom/DateTo
            if (saleDateDto.DateFrom.HasValue)
            {
                deliveryQuery = deliveryQuery.Where(d => d.SaleDate >= saleDateDto.DateFrom.Value);
                orderCountQuery = orderCountQuery.Where(oh => oh.OrderDate.HasValue && oh.OrderDate.Value >= saleDateDto.DateFrom.Value);
                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue && od.OrderHeader.OrderDate.Value >= saleDateDto.DateFrom.Value);
            }
            if (saleDateDto.DateTo.HasValue)
            {
                deliveryQuery = deliveryQuery.Where(d => d.SaleDate <= saleDateDto.DateTo.Value);
                orderCountQuery = orderCountQuery.Where(oh => oh.OrderDate.HasValue && oh.OrderDate.Value <= saleDateDto.DateTo.Value);
                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue && od.OrderHeader.OrderDate.Value <= saleDateDto.DateTo.Value);
            }

            // ✅ เพิ่มการกรองปี/เดือน สำหรับ Deliveries
            if (saleDateDto.Year.HasValue && saleDateDto.Month.HasValue)
            {
                deliveryQuery = deliveryQuery.Where(d => d.SaleDate.Year == saleDateDto.Year.Value &&
                                       d.SaleDate.Month == saleDateDto.Month.Value);

                orderCountQuery = orderCountQuery.Where(oh => oh.OrderDate.HasValue &&
                                        oh.OrderDate.Value.Year == saleDateDto.Year.Value &&
                                        oh.OrderDate.Value.Month == saleDateDto.Month.Value);

                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue &&
                                            od.OrderHeader.OrderDate.Value.Year == saleDateDto.Year.Value &&
                                            od.OrderHeader.OrderDate.Value.Month == saleDateDto.Month.Value);
            }
            else if (saleDateDto.Year.HasValue)
            {
                deliveryQuery = deliveryQuery.Where(d => d.SaleDate.Year == saleDateDto.Year.Value);

                orderCountQuery = orderCountQuery.Where(oh => oh.OrderDate.HasValue &&
                                            oh.OrderDate.Value.Year == saleDateDto.Year.Value);

                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue &&
                                                od.OrderHeader.OrderDate.Value.Year == saleDateDto.Year.Value);
            }
            else if (saleDateDto.Month.HasValue)
            {
                deliveryQuery = deliveryQuery.Where(d => d.SaleDate.Month == saleDateDto.Month.Value);

                orderCountQuery = orderCountQuery.Where(oh => oh.OrderDate.HasValue &&
                                            oh.OrderDate.Value.Month == saleDateDto.Month.Value);

                orderDetailsQuery = orderDetailsQuery.Where(od => od.OrderHeader!.OrderDate.HasValue &&
                                                od.OrderHeader.OrderDate.Value.Month == saleDateDto.Month.Value);
            }

            // ✅ ดึงข้อมูลจาก Deliveries
            var deliveryData = await deliveryQuery
                .Select(d => new { d.SaleDate, d.NetSales })
                .ToListAsync();

            // ✅ ดึงข้อมูลนับจำนวน Orders จาก OrderHeaders
            var orderCounts = await orderCountQuery
                .GroupBy(oh => oh.OrderDate)
                .Select(g => new { SaleDate = g.Key, OrderCount = g.Count() })
                .ToListAsync();

            // ✅ ดึงข้อมูล order headers สำหรับ peak hours calculation
            var allOrderHeaders = await orderCountQuery.ToListAsync();

            // ✅ คำนวณ Peak Hours สำหรับแต่ละวัน - ทุกช่วงที่มีออเดอร์
            var dailyPeakHours = allOrderHeaders
                .Where(oh => oh.OrderDate.HasValue && oh.OrderTime.HasValue)
                .GroupBy(oh => oh.OrderDate!.Value)
                .ToDictionary(dateGroup => dateGroup.Key, dateGroup =>
                {
                    // หาช่วงเวลาที่มีออเดอร์
                    var ordersWithTime = dateGroup.Where(oh => oh.OrderTime.HasValue).ToList();

                    if (!ordersWithTime.Any())
                        return new List<PeakHourDto>();

                    // หาช่วงเวลาตั้งแต่ออเดอร์แรกถึงออเดอร์สุดท้าย
                    var firstOrderHour = ordersWithTime.Min(oh => oh.OrderTime!.Value.Hour);
                    var lastOrderHour = ordersWithTime.Max(oh => oh.OrderTime!.Value.Hour);

                    // หายอดรวมในวันนั้นจาก Deliveries table
                    var dailyTotalSales = deliveryData
                        .Where(dd => dd.SaleDate == dateGroup.Key)
                        .Sum(dd => dd.NetSales);

                    // สร้างรายการ PeakHour สำหรับทุกช่วงเวลาที่มีออเดอร์
                    var peakHours = new List<PeakHourDto>();

                    for (int hour = firstOrderHour; hour <= lastOrderHour; hour++)
                    {
                        var ordersInHour = ordersWithTime
                            .Where(oh => oh.OrderTime!.Value.Hour == hour)
                            .ToList();

                        // เฉพาะช่วงที่มีออเดอร์เท่านั้น
                        if (ordersInHour.Any())
                        {
                            var hourlyOrderCount = ordersInHour.Count;

                            // ประมาณการยอดขายในชั่วโมงนั้นตามสัดส่วนจำนวนออเดอร์
                            var hourlyRevenue = dateGroup.Count() > 0
                                ? dailyTotalSales * hourlyOrderCount / dateGroup.Count()
                                : 0;

                            peakHours.Add(new PeakHourDto
                            {
                                HourRange = $"{hour:D2}:00-{(hour + 1):D2}:00",
                                OrderCount = hourlyOrderCount,
                                TotalSales = hourlyRevenue,

                                // ✅ AvgPerOrder = ยอดรวมในชั่วโมง ÷ จำนวน order ในชั่วโมง
                                AvgPerOrder = Math.Round(
                                    (double)(hourlyOrderCount > 0 ? hourlyRevenue / hourlyOrderCount : 0), 2)
                            });
                        }
                    }

                    // เรียงตามจำนวนออเดอร์มากที่สุด
                    return peakHours.OrderByDescending(x => x.OrderCount).ToList();
                });

            // ✅ ดึงข้อมูล order details ครั้งเดียวแล้วจัดกลุ่มใน memory (สำหรับ TopSellingItems)
            var allOrderDetails = await orderDetailsQuery.ToListAsync();

            // ✅ Group by date and menu, then calculate top selling items for each date
            var dailyMenuSales = allOrderDetails
                .Where(od => od.OrderHeader.OrderDate.HasValue)
                .GroupBy(od => od.OrderHeader.OrderDate!.Value)
                .ToDictionary(dateGroup => dateGroup.Key, dateGroup =>
                    dateGroup
                        .GroupBy(od => new { od.MenuId, MenuName = od.Menu?.Name ?? "Unknown" })
                        .Select(menuGroup => new SoldMenuDto
                        {
                            MenuId = menuGroup.Key.MenuId,
                            MenuName = menuGroup.Key.MenuName,
                            QuantitySold = menuGroup.Sum(od => od.Quantity),
                            TotalSales = menuGroup.Sum(od => od.Price * od.Quantity),
                            TotalCost = menuGroup.Sum(od => (od.Menu?.Cost ?? 0) * od.Quantity),
                            TotalProfit = menuGroup.Sum(od => (od.Price - (od.Menu?.Cost ?? 0)) * od.Quantity),
                            ProfitMargin = menuGroup.Sum(od => od.Price * od.Quantity) > 0 ?
                                (double)(menuGroup.Sum(od => (od.Price - (od.Menu?.Cost ?? 0)) * od.Quantity) /
                                menuGroup.Sum(od => od.Price * od.Quantity) * 100) : 0
                        })
                        .OrderByDescending(x => x.QuantitySold)
                        //.Take(5) // ✅ เอา 5 รายการแรกที่ขายดีที่สุด
                        .ToList()
                );

            // ✅ Join ข้อมูลจาก Deliveries และ OrderHeaders พร้อมเพิ่ม PeakHours
            var dailySales = deliveryData
                .GroupBy(d => d.SaleDate)
                .Select(g =>
                {
                    var orderCount = orderCounts
                    .FirstOrDefault(oc => oc.SaleDate == g.Key)?.OrderCount ?? 0;

                    var totalNetSales = g.Sum(x => x.NetSales);

                    return new DailySaleDto
                    {
                        SaleDate = g.Key,
                        Orders = orderCount, // ✅ จำนวนจาก OrderHeaders
                        TotalAmount = totalNetSales,
                        AvgPerOrder = orderCount > 0
                            ? Math.Round((double)(totalNetSales / orderCount), 2)
                            : 0,
                        TopSellingItems = new List<SoldMenuDto>(), // ✅ Initialize empty list
                        PeakHours = new List<PeakHourDto>(), // ✅ Initialize empty list
                        totalOrders = orderCount
                    };
                })
                .OrderByDescending(x => x.SaleDate)
                .ToList();

            // ✅ Assign ข้อมูลเพิ่มเติมให้แต่ละ daily sale
            foreach (var dailySale in dailySales)
            {
                if (dailySale.SaleDate != null)
                {
                    // Assign top selling items
                    if (dailyMenuSales.TryGetValue(dailySale.SaleDate.Value, out var topItems))
                    {
                        dailySale.TopSellingItems = topItems;
                    }

                    // ✅ Assign peak hours (ทุกช่วงที่มีออเดอร์)
                    if (dailyPeakHours.TryGetValue(dailySale.SaleDate.Value, out var peakHours))
                    {
                        dailySale.PeakHours = peakHours;
                    }
                }
            }

            // ✅ เพิ่ม logging สำหรับ debug
            _logger.LogInformation($"📊 GetDailyDeliverySalesReport: Found {dailySales.Count} records" +
                $" | Year: {saleDateDto.Year}" +
                $" | Month: {saleDateDto.Month}");

            return dailySales;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ เกิดข้อผิดพลาดขณะดึงรายงานยอดขายเดลิเวอรี่");
            if (ex.InnerException != null)
            {
                _logger.LogError("🔥 InnerException: " + ex.InnerException.Message);
            }
            return new List<DailySaleDto>();
        }

    }
    public async Task<string> UpdateDeliveryRecords(DeliveryDto deliveryDto)
    {
        try
        {
            var existingRecord = await _context.Deliveries.FirstOrDefaultAsync(d => d.SaleDate == deliveryDto.SaleDate);
            if (existingRecord != null)
            {
                // อัปเดตข้อมูลที่มีอยู่
                existingRecord.TotalSales = deliveryDto.TotalSales;
                existingRecord.NetSales = deliveryDto.NetSales;
                existingRecord.GPPercent = deliveryDto.GPPercent;
                existingRecord.GPAmount = deliveryDto.GPAmount;
                existingRecord.UpdateDate = _utilService.GetThailandDate();
                existingRecord.UpdateTime = _utilService.GetThailandTime();
                existingRecord.UpdatedBy = deliveryDto.UpdatedBy;
                existingRecord.Active = true;

                _context.Deliveries.Update(existingRecord);
                await _context.SaveChangesAsync();
                return $"✅ อัปเดตข้อมูลการจัดส่งสำหรับวันที่ {deliveryDto.SaleDate} เรียบร้อยแล้ว";
            }
            else
            {
                // สร้างระเบียนใหม่
                var newRecord = new Delivery
                {
                    SaleDate = deliveryDto.SaleDate,
                    TotalSales = deliveryDto.TotalSales,
                    NetSales = deliveryDto.NetSales,
                    GPPercent = deliveryDto.GPPercent,
                    GPAmount = deliveryDto.GPAmount,
                    UpdateDate = _utilService.GetThailandDate(),
                    UpdateTime = _utilService.GetThailandTime(),
                    UpdatedBy = deliveryDto.UpdatedBy,
                    Active = true
                };

                _context.Deliveries.Add(newRecord);
                await _context.SaveChangesAsync();
                return $"✅ เพิ่มข้อมูลการจัดส่งสำหรับวันที่ {deliveryDto.SaleDate} เรียบร้อยแล้ว";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ เกิดข้อผิดพลาดขณะอัปเดตข้อมูลการจัดส่ง");
            if (ex.InnerException != null)
            {
                _logger.LogError("🔥 InnerException: " + ex.InnerException.Message);
            }
            return "❌ เกิดข้อผิดพลาด: " + ex.Message;
        }
    }
    public async Task<List<DeliveryDto>> GetDeliveryRecords(DeliveryDto deliveryDto)
    {
        var query = _context.Deliveries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(deliveryDto.SelectedMonth) && int.TryParse(deliveryDto.SelectedMonth, out int month))
            query = query.Where(d => d.SaleDate.Month == month);

        if (!string.IsNullOrWhiteSpace(deliveryDto.SelectedYear) && int.TryParse(deliveryDto.SelectedYear, out int year))
            query = query.Where(d => d.SaleDate.Year == year);

        var records = await query
            .OrderByDescending(d => d.SaleDate)
            .ToListAsync();

        if (!records.Any())
            return new List<DeliveryDto>();

        // ✅ ดึงวันที่ทั้งหมดเพื่อใช้ query OrderDetails ครั้งเดียว
        var saleDates = records.Select(d => d.SaleDate).ToList();

        // ✅ ดึง OrderDetails พร้อม Menu ของ delivery ในช่วงวันที่นั้น
        var orderDetails = await _context.OrderDetails
            .Include(od => od.Menu)
            .Include(od => od.Toppings).ThenInclude(t => t.Menu)
            .Where(od => od.OrderHeader != null
                      && od.OrderHeader.OrderTypeId == 3
                      && od.OrderHeader.OrderDate.HasValue
                      && saleDates.Contains(od.OrderHeader.OrderDate!.Value))
            .Select(od => new
            {
                OrderDate  = od.OrderHeader!.OrderDate!.Value,
                MenuCost   = od.Menu != null ? od.Menu.Cost : 0,
                ToppingCost = od.Toppings != null
                    ? od.Toppings.Sum(t => t.Menu != null ? t.Menu.Cost : 0)
                    : 0,
                od.Quantity
            })
            .ToListAsync();

        // ✅ รวม FoodCost แยกตามวันที่
        var foodCostByDate = orderDetails
            .GroupBy(od => od.OrderDate)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(od => (od.MenuCost + od.ToppingCost) * od.Quantity)
            );

        // ✅ นับจำนวน order แยกตามวันที่
        var orderCountByDate = await _context.OrderHeaders
            .Where(oh => oh.OrderTypeId == 3
                      && oh.OrderDate.HasValue
                      && saleDates.Contains(oh.OrderDate!.Value))
            .GroupBy(oh => oh.OrderDate!.Value)
            .Select(g => new { SaleDate = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SaleDate, x => x.Count);

        // ✅ แพ็กข้อมูลใส่ DTO
        var result = records.Select(d => new DeliveryDto
        {
            DeliveryId   = d.DeliveryId,
            SaleDate     = d.SaleDate,
            TotalSales   = d.TotalSales,
            NetSales     = d.NetSales,
            GPPercent    = d.GPPercent,
            GPAmount     = d.GPAmount,
            UpdatedBy    = d.UpdatedBy,
            TotalOrders  = orderCountByDate.GetValueOrDefault(d.SaleDate, 0),
            TotalFoodCost = foodCostByDate.GetValueOrDefault(d.SaleDate, 0) // ✅ FoodCost รวมของวันนั้น
        }).ToList();

        return result;
    }

    public async Task<List<DeliveryOrdersDTO>> GetDeliveryOrdersByDate(DeliveryDto deliveryDto)
    {
        if (deliveryDto.SaleDate == DateOnly.MinValue)
            return new List<DeliveryOrdersDTO>();

        try
        {
            // 1) ดึง OrderHeaders เฉพาะเดลิเวอรี่ของวันที่เลือก
            var orderHeaders = await (
                from oh in _context.OrderHeaders
                join ot in _context.Ordertypes on oh.OrderTypeId equals ot.OrderTypeId into otGroup
                from ot in otGroup.DefaultIfEmpty()
                where oh.OrderDate == deliveryDto.SaleDate && oh.OrderTypeId == 3
                orderby oh.OrderTime descending
                select new { OrderHeader = oh, OrderTypeName = ot != null ? ot.OrderTypeName : "ไม่ระบุ" }
            ).ToListAsync();

            if (!orderHeaders.Any())
                return new List<DeliveryOrdersDTO>();

            // 2) ดึง OrderDetails พร้อม Menu และ Toppings
            var orderIds     = orderHeaders.Select(x => x.OrderHeader.OrderId).ToList();
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Menu)
                .Include(od => od.Toppings).ThenInclude(t => t.Menu)
                .Where(od => orderIds.Contains(od.OrderId))
                .ToListAsync();

            // 3) จัดกลุ่ม OrderDetails ตาม OrderId เพื่อให้ lookup ง่าย
            var detailsByOrderId = orderDetails
                .GroupBy(od => od.OrderId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4) สร้าง DTO แพ็กใส่ตัวแปรก่อน return
            var result = orderHeaders.Select(item =>
            {
                var oh      = item.OrderHeader;
                var details = detailsByOrderId.GetValueOrDefault(oh.OrderId, new List<OrderDetail>());

                // คำนวณ helper function สำหรับ cost และ grab price
                decimal CalFoodCost(OrderDetail od) =>
                    ((od.Menu?.Cost ?? 0) + (od.Toppings?.Sum(t => t.Menu?.Cost ?? 0) ?? 0)) * od.Quantity;

                decimal CalGrabPrice(OrderDetail od) =>
                    ((od.Menu?.GrabPrice ?? 0) + (od.Toppings?.Sum(t => t.Menu?.GrabPrice ?? 0) ?? 0)) * od.Quantity;

                var orderDetailDtos = details.Select(od => new OrderDetailDTO
                {
                    OrderDetailId     = od.OrderDetailId,
                    OrderId           = od.OrderId,
                    MenuId            = od.MenuId,
                    MenuName          = od.Menu?.Name ?? "ไม่ทราบชื่อเมนู",
                    Quantity          = od.Quantity,
                    Price             = od.Price,
                    GrabPrice         = (od.Menu?.GrabPrice ?? 0) + (od.Toppings?.Sum(t => t.Menu?.GrabPrice ?? 0) ?? 0),
                    TotalFoodCost     = CalFoodCost(od),
                    ToppingQTY        = od.ToppingQTY,
                    MenuIdInFirestore  = od.MenuIdInFirestore,
                    IsDone            = od.IsDone,
                    IsDischarge       = od.IsDischarge,
                    Remark            = od.Remark,
                    Toppings          = od.Toppings?.Select(t => new OrderDetailToppingDTO
                    {
                        OrderDetailToppingId = t.OrderDetailToppingId,
                        OrderDetailId        = t.OrderDetailId,
                        MenuId               = t.MenuId,
                        toppingNames         = t.Menu?.Name ?? "ไม่ทราบชื่อท็อปปิ้ง",
                        ToppingPrice         = t.ToppingPrice,
                        TotalGrabPrice       = t.Menu?.GrabPrice ?? 0
                    }).ToList() ?? new List<OrderDetailToppingDTO>()
                }).ToList();

                return new DeliveryOrdersDTO
                {
                    OrderId         = oh.OrderId,
                    CustomerName    = oh.CustomerName ?? "ไม่ระบุชื่อ",
                    OrderDate       = oh.OrderDate,
                    OrderTime       = oh.OrderTime,
                    OrderTypeId     = oh.OrderTypeId,
                    OrderTypeName   = item.OrderTypeName,
                    DischargeTime   = oh.DischargeTime,
                    IsDischarge     = oh.IsDischarge,
                    FinishOrderTime = oh.FinishOrderTime,
                    IsFinishOrder   = oh.IsFinishOrder,
                    TotalPrice      = oh.TotalPrice,
                    TotalGrabPrice  = details.Sum(od => CalGrabPrice(od)),
                    TotalFoodCost   = details.Sum(od => CalFoodCost(od)),
                    OrderRemark     = oh.OrderRemark ?? string.Empty,
                    ItemQTY         = oh.ItemQTY,
                    OrderDetails    = orderDetailDtos
                };
            }).ToList();

            _logger.LogInformation(
                "📋 Retrieved {Count} delivery orders for {Date}",
                result.Count, deliveryDto.SaleDate);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting delivery orders for date {Date}", deliveryDto.SaleDate);
            throw new InvalidOperationException(
                $"ไม่สามารถดึงข้อมูลคำสั่งซื้อสำหรับวันที่ {deliveryDto.SaleDate} ได้", ex);
        }
    }
    public async Task<List<IncomeOrdersDTO>> GetIncomeOrdersByDate(IncomeDto incomeDto)
    {
        if (incomeDto.SaleDate == DateOnly.MinValue)
        {
            return new List<IncomeOrdersDTO>();
        }

        try
        {
            // ✅ ส่วนเดิมทั้งหมด - ไม่เปลี่ยน
            var orderData = await _context.OrderHeaders
            .Include(oh => oh.OrderType)      // ✅ ใช้ Include แทน Join
            .Include(oh => oh.Discount)       // ✅ เพิ่ม Discount สำหรับ DiscountName
            .Where(oh => oh.OrderDate == incomeDto.SaleDate && oh.OrderTypeId != 3)
            .OrderByDescending(oh => oh.OrderTime)
            .Select(oh => new
            {
                OrderHeader = oh,
                OrderTypeName = oh.OrderType != null ? oh.OrderType.OrderTypeName : "ไม่ระบุ"
            })
            .ToListAsync();

            var orderIds = orderData.Select(x => x.OrderHeader.OrderId).ToList();

            var orderDetails = await _context.OrderDetails
                .Include(od => od.Menu)
                .Include(od => od.Toppings)
                    .ThenInclude(t => t.Menu)
                .Where(od => orderIds.Contains(od.OrderId))
                .ToListAsync();

            var detailsLookup = orderDetails.GroupBy(od => od.OrderId)
                                  .ToDictionary(g => g.Key, g => g.ToList());

            // ✅ เปลี่ยนจาก Task.WhenAll เป็น foreach sequential
            var discountLookup = new Dictionary<int, decimal>();

            var ordersWithDiscount = orderData
                .Where(item => item.OrderHeader.DiscountID != null && item.OrderHeader.DiscountID != 0)
                .ToList();

            foreach (var item in ordersWithDiscount)
            {
                try
                {
                    var discountAmount = await CalDiscountPrice(item.OrderHeader);
                    discountLookup[item.OrderHeader.OrderId] = discountAmount;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Error calculating discount for OrderId {item.OrderHeader.OrderId}");
                    discountLookup[item.OrderHeader.OrderId] = 0;
                }
            }

            // ✅ ส่วนที่เหลือเหมือนเดิม
            var incomeOrders = orderData.Select(item =>
            {
                var oh = item.OrderHeader;
                var details = detailsLookup.GetValueOrDefault(oh.OrderId, new List<OrderDetail>());

                // คำนวณ TotalGrabPrice
                var totalGrabPrice = details.Sum(od =>
                {
                    var mainGrabPrice = (od.Menu?.GrabPrice ?? 0) * od.Quantity;
                    var toppingsGrabPrice = (od.Toppings?.Sum(t => t.Menu?.GrabPrice ?? 0) ?? 0) * od.Quantity;
                    return mainGrabPrice + toppingsGrabPrice;
                });

                // ✅ ใช้ discountLookup
                decimal priceAfterDiscount = 0;
                decimal discountAmount = discountLookup.GetValueOrDefault(oh.OrderId, 0);

                string DiscountName = "";
                if (oh.DiscountID != null && oh.DiscountID != 0)
                {
                    priceAfterDiscount = oh.TotalPrice - discountAmount;
                    DiscountName = oh.Discount?.Description ?? string.Empty;
                }
                else
                {
                    priceAfterDiscount = oh.TotalPrice;
                }

                // ✅ ส่วนที่เหลือทั้งหมด - ไม่เปลี่ยน
                var result = new IncomeOrdersDTO
                {
                    OrderId = oh.OrderId,
                    CustomerName = oh.CustomerName ?? "ไม่ระบุชื่อ",
                    OrderDate = oh.OrderDate,
                    OrderTime = oh.OrderTime,
                    OrderTypeId = oh.OrderTypeId,
                    OrderTypeName = item.OrderTypeName,
                    DischargeTime = oh.DischargeTime,
                    IsDischarge = oh.IsDischarge,
                    FinishOrderTime = oh.FinishOrderTime,
                    IsFinishOrder = oh.IsFinishOrder,
                    TotalPrice = priceAfterDiscount, // ✅ ใช้ priceAfterDiscount
                    TotalGrabPrice = totalGrabPrice,
                    OrderRemark = oh.OrderRemark ?? string.Empty,
                    ItemQTY = oh.ItemQTY,
                    DiscountPrice = discountAmount,
                    DiscountID = oh.DiscountID,
                    DiscountName = DiscountName,
                    OrderDetails = details.Select(od =>
                    {
                        var menuGrabPrice = od.Menu?.GrabPrice ?? 0;
                        var toppingsGrabPrice = od.Toppings?.Sum(t => t.Menu?.GrabPrice ?? 0) ?? 0;
                        var totalGrabPricePerItem = menuGrabPrice + toppingsGrabPrice;

                        return new OrderDetailDTO
                        {
                            OrderDetailId = od.OrderDetailId,
                            OrderId = od.OrderId,
                            MenuId = od.MenuId,
                            MenuName = od.Menu?.Name ?? "ไม่ทราบชื่อเมนู",
                            Quantity = od.Quantity,
                            Price = od.Price,
                            GrabPrice = totalGrabPricePerItem,
                            ToppingQTY = od.ToppingQTY,
                            MenuIdInFirestore = od.MenuIdInFirestore,
                            IsDone = od.IsDone,
                            IsDischarge = od.IsDischarge,
                            Remark = od.Remark,

                            Toppings = od.Toppings?.Select(t => new OrderDetailToppingDTO
                            {
                                OrderDetailToppingId = t.OrderDetailToppingId,
                                OrderDetailId = t.OrderDetailId,
                                MenuId = t.MenuId,
                                toppingNames = t.Menu?.Name ?? "ไม่ทราบชื่อท็อปปิ้ง",
                                ToppingPrice = t.ToppingPrice,
                                TotalGrabPrice = t.Menu?.GrabPrice ?? 0
                            }).ToList() ?? new List<OrderDetailToppingDTO>()
                        };
                    }).ToList()
                };

                return result;
            }).ToList();

            _logger.LogInformation($"📋 Retrieved {incomeOrders.Count} income orders for {incomeDto.SaleDate}");

            return incomeOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error getting income orders for date {incomeDto.SaleDate}");
            throw new InvalidOperationException($"ไม่สามารถดึงข้อมูลคำสั่งซื้อสำหรับวันที่ {incomeDto.SaleDate} ได้", ex);
        }
    }
    private async Task<decimal> CalDiscountPrice(OrderHeader orderHeader)
    {
        try
        {

            if (orderHeader.DiscountID == null || orderHeader.DiscountID == 0)
            {
                return 0;
            }

            decimal discountAmount = 0;
            switch (orderHeader.DiscountID)
            {
                case 1:// คิดส่วนลด 10%                
                    discountAmount = (10 / 100) * orderHeader.TotalPrice;
                    break;
                case 2:// คิดส่วนลด 20%
                    discountAmount = (20 / 100) * orderHeader.TotalPrice;
                    break;
                case 3:// คิดส่วนลด 50%
                    discountAmount = (50 / 100) * orderHeader.TotalPrice;
                    break;
                case 5:// ส่วนลดตามวงล้อ
                    var EventRollingResults = await _context.EventRollingResults
                        .Where(e => e.OrderFirstStoreID == orderHeader.IdInFirestore)
                        .Include(e => e.Reward)
                        .FirstOrDefaultAsync();
                    switch (EventRollingResults!.RewardID)
                    {
                        case 1:// คิดส่วนลด 100%
                            discountAmount = orderHeader.TotalPrice;
                            break;
                        case 2:// คิดส่วนลด 50%
                            discountAmount = 0.50m * orderHeader.TotalPrice;
                            break;
                        case 23:// คิดส่วนลด 10%
                            discountAmount = 0.10m * orderHeader.TotalPrice;
                            break;
                        default:
                            discountAmount = EventRollingResults.Reward!.CostPrice;
                            break;
                    }
                    break;
                default:
                    discountAmount = 0;
                    break;
            }
            // ❌ ลบส่วนนี้ออก - ไม่ต้อง update database ใน read operation
            if (discountAmount > 0)
            {
                await UpdateDiscountPrice(orderHeader.OrderId, discountAmount);
            }

            return Math.Round(discountAmount, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error calculating discount for OrderId {orderHeader.OrderId}");
            return 0;
        }
    }

    // ฟังก์ชัน update แยก
    private async Task UpdateDiscountPrice(int orderId, decimal discountAmount)
    {
        try
        {
            var order = await _context.OrderHeaders.FindAsync(orderId);
            if (order != null && discountAmount > 0)
            {
                if (order.DiscountPrice == discountAmount)
                {
                    // ถ้าเท่ากัน ไม่ต้องอัปเดต
                    return;
                }
                order.DiscountPrice = discountAmount;
                _context.OrderHeaders.Update(order);
                await _context.SaveChangesAsync(); // ✅ Save ที่นี่
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error updating discount price for OrderId {orderId}");
        }
    }
    public async Task<List<CategorySaleDto>> GetSaleOfMenu(int year, int month)
    {
        try
        {
            // ✅ 1. ดึงเมนูที่ Active ทั้งหมด
            var activeMenus = await _context.Menus
                .Where(m => m.Active == true)
                .Include(m => m.Category)
                .ToListAsync();

            // ✅ 2. สร้าง query พื้นฐาน
            var baseQuery = _context.OrderDetails
                .Include(od => od.OrderHeader)
                .Where(od => od.OrderHeader != null
                    && od.OrderHeader.OrderDate.HasValue
                    && od.OrderHeader.OrderDate.Value.Year == year);

            // ✅ 3. เพิ่มเงื่อนไขเดือน (ถ้ามี)
            if (month > 0)
            {
                baseQuery = baseQuery.Where(od => od.OrderHeader.OrderDate.HasValue && od.OrderHeader.OrderDate.Value.Month == month);
            }

            // ✅ 4. ดึงข้อมูลการขายหน้าร้าน (ไม่รวม delivery)
            var dineInSales = await baseQuery
                .Where(od => od.OrderHeader.OrderTypeId != 3) // หน้าร้าน
                .GroupBy(od => od.MenuId)
                .Select(g => new { MenuId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.MenuId, x => x.Qty);

            // ✅ 5. ดึงข้อมูลการขาย Delivery
            var deliverySales = await baseQuery
                .Where(od => od.OrderHeader.OrderTypeId == 3) // delivery
                .GroupBy(od => od.MenuId)
                .Select(g => new { MenuId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.MenuId, x => x.Qty);

            // ✅ 6. สร้าง list เมนูพร้อมจำนวนขาย
            var menuSales = activeMenus.Select(menu => new MenuSaleSimpleDto
            {
                MenuId = menu.Id,
                MenuName = menu.Name,
                CategoryId = menu.CategoryId ?? 0,
                CategoryName = menu.Category?.CategoryName ?? "ไม่ระบุ",
                DineInQty = dineInSales.GetValueOrDefault(menu.Id, 0),
                DeliveryQty = deliverySales.GetValueOrDefault(menu.Id, 0),
                TotalQty = dineInSales.GetValueOrDefault(menu.Id, 0) +
                          deliverySales.GetValueOrDefault(menu.Id, 0)
            })
            .OrderByDescending(x => x.TotalQty)
            .ToList();

            // ✅ 7. แบ่งกลุ่มตาม Category
            var categories = menuSales
                .GroupBy(m => new { m.CategoryId, m.CategoryName })
                .Select(g => new CategorySaleDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Menus = g.ToList()
                })
                .OrderBy(x => x.CategoryName)
                .ToList();

            // ✅ 8. Log ที่ปรับปรุง
            var period = month > 0 
                ? $"{year}/{month:D2}" 
                : $"{year} (ทั้งปี)";
        
            _logger.LogInformation($"📊 GetSaleOfMenu: {period} | {menuSales.Count} menus in {categories.Count} categories");

            return categories;
        }
        catch (Exception ex)
        {
            var period = month > 0 ? $"{year}/{month:D2}" : $"{year}";
            _logger.LogError(ex, $"❌ Error: {period}");
            return new List<CategorySaleDto>();
        }
    }
}