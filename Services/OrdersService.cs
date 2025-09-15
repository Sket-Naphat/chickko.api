using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using chickko.api.Data;
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
    public async Task<List<DailySaleDto>> GetDailyDineInSalesReport(DateOnly date)
    {
        var dailySales = await _context.OrderHeaders
            .Where(oh => oh.OrderTypeId != 3)   // กรองยอดขายหน้าร้าน
                                                //.Where(oh => oh.IsFinishOrder == true) // ถ้าต้องการเฉพาะที่ปิดบิลแล้ว
            .GroupBy(oh => oh.OrderDate)
            .Select(g => new DailySaleDto
            {
                SaleDate = g.Key ?? DateOnly.MinValue,
                Orders = g.Count(),
                TotalAmount = g.Sum(x => (decimal?)x.TotalPrice) ?? 0,
                AvgPerOrder = Math.Round(
                                  (double)((decimal?)g.Average(x => (decimal?)x.TotalPrice) ?? 0), 2)
            })
            .OrderByDescending(x => x.SaleDate)
            .ToListAsync();

        return dailySales;
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
                existingRecord.UpdateDate = DateOnly.FromDateTime(System.DateTime.Now);
                existingRecord.UpdateTime = TimeOnly.FromDateTime(System.DateTime.Now);
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
                    UpdateDate = DateOnly.FromDateTime(System.DateTime.Now),
                    UpdateTime = TimeOnly.FromDateTime(System.DateTime.Now),
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
        {
            query = query.Where(d => d.SaleDate.Month == month);
        }

        if (!string.IsNullOrWhiteSpace(deliveryDto.SelectedYear) && int.TryParse(deliveryDto.SelectedYear, out int year))
        {
            query = query.Where(d => d.SaleDate.Year == year);
        }

        var records = await query
            .OrderByDescending(d => d.SaleDate)
            .Select(d => new DeliveryDto
            {
                DeliveryId = d.DeliveryId,
                SaleDate = d.SaleDate,
                TotalSales = d.TotalSales,
                NetSales = d.NetSales,
                GPPercent = d.GPPercent,
                GPAmount = d.GPAmount,
                UpdatedBy = d.UpdatedBy
            })
            .ToListAsync();

        return records;
    }
    // ...existing code...
    public async Task<List<DeliveryOrdersDTO>> GetDeliveryOrdersByDate(DeliveryDto deliveryDto)
    {
        if (deliveryDto.SaleDate == DateOnly.MinValue)
        {
            return new List<DeliveryOrdersDTO>();
        }

        try
        {
            // ใช้ SQL Query ที่ optimize แล้ว
            var query = from oh in _context.OrderHeaders
                        join ot in _context.Ordertypes on oh.OrderTypeId equals ot.OrderTypeId into otGroup
                        from ot in otGroup.DefaultIfEmpty()
                        where oh.OrderDate == deliveryDto.SaleDate 
                              && oh.OrderTypeId == 3 // เฉพาะเดลิเวอรี่
                        orderby oh.OrderTime descending
                        select new
                        {
                            OrderHeader = oh,
                            OrderTypeName = ot != null ? ot.OrderTypeName : "ไม่ระบุ"
                        };

            var orderData = await query.ToListAsync();

            // ดึง OrderDetails แยกเพื่อ performance
            var orderIds = orderData.Select(x => x.OrderHeader.OrderId).ToList();

            var orderDetails = await _context.OrderDetails
                .Include(od => od.Menu)
                .Include(od => od.Toppings)
                    .ThenInclude(t => t.Menu)
                .Where(od => orderIds.Contains(od.OrderId))
                .ToListAsync();

            // Group OrderDetails by OrderId
            var detailsLookup = orderDetails.GroupBy(od => od.OrderId)
                                          .ToDictionary(g => g.Key, g => g.ToList());

            // สร้าง DTO
            var deliveryOrders = orderData.Select(item =>
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

                var result = new DeliveryOrdersDTO
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
                    TotalPrice = oh.TotalPrice,
                    TotalGrabPrice = totalGrabPrice,
                    OrderRemark = oh.OrderRemark ?? string.Empty,
                    ItemQTY = oh.ItemQTY,

                    OrderDetails = details.Select(od => 
                    {
                        // คำนวณ GrabPrice รวมของเมนูหลักและ toppings
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
                            
                            // GrabPrice รวม = เมนูหลัก + ท็อปปิ้งทั้งหมด (ต่อ 1 ชิ้น)
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

            _logger.LogInformation($"📋 Retrieved {deliveryOrders.Count} delivery orders for {deliveryDto.SaleDate}");

            // Log สรุปยอดขาย
            var totalOrders = deliveryOrders.Count;
            var totalRevenue = deliveryOrders.Sum(x => x.TotalPrice);
            var totalGrabRevenue = deliveryOrders.Sum(x => x.TotalGrabPrice);

            _logger.LogInformation($"💰 Summary for {deliveryDto.SaleDate}: {totalOrders} orders, Revenue: ฿{totalRevenue:N2}, Grab Revenue: ฿{totalGrabRevenue:N2}");

            return deliveryOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error getting delivery orders for date {deliveryDto.SaleDate}");
            throw new InvalidOperationException($"ไม่สามารถดึงข้อมูลคำสั่งซื้อสำหรับวันที่ {deliveryDto.SaleDate} ได้", ex);
        }
    }
    // ...existing code...
}