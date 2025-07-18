using chickko.api.Data;
using chickko.api.Interface;
using chickko.api.Models;
using chickko.api.Services;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;
public class OrdersService : IOrdersService
{
    private readonly ChickkoContext _context;
    private readonly ILogger<OrdersService> _logger;
    private readonly IUtilService _utilService;


    public class MockDocument
    {
        public string Id { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public Dictionary<string, object> ToDictionary() => Data;
    }
    public OrdersService(ChickkoContext context, ILogger<OrdersService> logger, IUtilService utilService)
    {
        _context = context;
        _logger = logger;
        _utilService = utilService;
    }

    // ✅ คุณต้องเขียนเองให้เชื่อมกับ Firestore SDK
    // และทำการคัดลอกข้อมูลจาก Firestore มายังฐานข้อมูล ChickkoContext
    public async Task<string> CopyOrderFromFirestore(string datefrom = "", string dateto = "")
    {
        int copied = 0;
        string errorFromFirestoreID = "";
        string errorCustomName = "";
        string errorDischargeType = "";
        try
        {
            var snapshot = await _utilService.GetSnapshotFromFirestoreWithFiltersBetween("orders", "orderDate", datefrom, dateto);

            #region commented-out-mock-data
            // var mockDocuments = new List<MockDocument>
            // {
            //     new MockDocument
            //     {
            //         Id = "007GnHvdsyUuqmMZ1BN7",
            //         Data = new Dictionary<string, object>
            //         {
            //             { "customerName", "โต้ะ3" },
            //             { "discharge", true },
            //             { "dischargeTime", "19:46:28" },
            //             { "dischargeType", "Promptpay" },
            //             { "finishedOrder", true },
            //             { "finishedOrderTime", "19:57:24" },
            //             { "locationOrder", "forHere" },
            //             { "orderDate", "2025-07-04" },
            //             { "orderTime", "19:46:05" },
            //             { "remark", "" },
            //             { "tableNumber", "t3" },
            //             { "items", new List<Dictionary<string, object>>
            //                 {
            //                     new Dictionary<string, object>
            //                     {
            //                         { "done", true },
            //                         { "id", "hM8SHkyb8ZVezWCxR1ax+5gxjT8tCq42gUGKaYZbD+Ixb6dxsgiU62yJTzvFka" },
            //                         { "is_done", false },
            //                         { "itemDischarge", true },
            //                         { "name", "รามยอนซอสเผ็ดไก่ทอด + ซอสเผ็ดสไตล์เกาหลี + ไข่กุ้ง" },
            //                         { "price", 184 },
            //                         { "quantity", 1 },
            //                         { "remark", "" }
            //                     },
            //                     new Dictionary<string, object>
            //                     {
            //                         { "done", true },
            //                         { "id", "l1CTBp1FecxclwX3IkAo" },
            //                         { "is_done", false },
            //                         { "itemDischarge", true },
            //                         { "name", "น้ำเปล่า" },
            //                         { "price", 10 },
            //                         { "quantity", 1 },
            //                         { "remark", "" }
            //                     },
            //                     new Dictionary<string, object>
            //                     {
            //                         { "done", true },
            //                         { "id", "w8KsuJ04OGru35Yk6NLg" },
            //                         { "is_done", false },
            //                         { "itemDischarge", true },
            //                         { "name", "สปาเก็ตตี่ผัดพริก +ไข่กุ้ง" },
            //                         { "price", 159 },
            //                         { "quantity", 1 },
            //                         { "remark", "" }
            //                     },
            //                     new Dictionary<string, object>
            //                     {
            //                         { "done", true },
            //                         { "id", "ZI8MUzVSAkYqE262OUgg" },
            //                         { "is_done", false },
            //                         { "itemDischarge", true },
            //                         { "name", "มะนาวโซดา" },
            //                         { "price", 45 },
            //                         { "quantity", 1 },
            //                         { "remark", "" }
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // };
            #endregion

            foreach (var doc in snapshot.Documents)
            {
                //select from orders where orderDate = doc.Data["orderDate"].ToString() and customerName = doc.Data["customerName"].ToString(); if exists then continue;


                var data = doc.ToDictionary();
                var existingOrder = _context.OrderHeaders
                    .FirstOrDefault(o => o.OrderDate.ToString() == data["orderDate"].ToString() &&
                                         o.CustomerName == data["customerName"].ToString());

                if (existingOrder != null)
                {
                    _logger.LogInformation($"Order already exists for {data["customerName"]} on {data["orderDate"]}. Skipping.");
                    continue; // ถ้าเจอแล้วข้ามไป
                }
                else
                {
                    string dischargeName = "";
                    if (data.TryGetValue("dischargeType", out var dischargeRaw) && dischargeRaw != null)
                    {
                        dischargeName = dischargeRaw.ToString().Trim();
                    }
                    string locName = data["locationOrder"]?.ToString() ?? "";
                    string tableNumber = data["tableNumber"]?.ToString() ?? "";
                    errorFromFirestoreID = doc.Id; // เก็บ ID ของเอกสาร Firestore เพื่อใช้ในกรณีเกิดข้อผิดพลาด
                    errorCustomName = data["customerName"]?.ToString() ?? "";
                    errorDischargeType = dischargeName;
                    if (string.IsNullOrEmpty(dischargeName) || string.IsNullOrEmpty(locName))
                    {
                        _logger.LogWarning($"ข้อมูลไม่ครบสำหรับ Order ID: {doc.Id}");
                        continue;
                    }

                    Ordertype _OrderType = _context.Ordertypes.FirstOrDefault(x => x.OrderTypeName == locName) ?? new Ordertype(); // ตรวจสอบว่า OrderType มีอยู่ในฐานข้อมูลหรือไม่


                    // ดึงจากฐานข้อมูล ถ้าไม่เจอชื่อที่ได้มา → fallback เป็น "Promptpay"
                    DischargeType _DischargeType = new DischargeType();
                    if (!string.IsNullOrEmpty(dischargeName))
                    {
                        _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == dischargeName);

                    }
                    else
                    {
                        _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Promptpay");
                    }
                    //DischargeType _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == dischargeName) ?? new DischargeType(); // ตรวจสอบว่า DischargeType มีอยู่ในฐานข้อมูลหรือไม่
                    Table _Table = _context.Tables.FirstOrDefault(x => x.TableName == tableNumber) ?? new Table(); // ตรวจสอบว่า Table มีอยู่ในฐานข้อมูลหรือไม่
                    var order = new OrderHeader
                    {
                        CustomerName = data["customerName"]?.ToString() ?? "",
                        OrderDate = DateOnly.TryParse(data["orderDate"]?.ToString(), out var orderDate) ? orderDate : null,
                        OrderTime = TimeOnly.TryParse(data["orderTime"]?.ToString(), out var orderTime) ? orderTime : null,
                        OrderTypeId = _OrderType?.OrderTypeId ?? 0,
                        OrderType = _OrderType ?? new Ordertype() { OrderTypeName = locName }, // ใช้ค่าเริ่มต้นถ้าไม่พบ
                        DischargeTypeId = _DischargeType.DischargeTypeId,
                        DischargeType = _DischargeType, // ใช้ค่าเริ่มต้นถ้าไม่พบ
                        DischargeTime = TimeOnly.TryParse(data["dischargeTime"]?.ToString(), out var dTime) ? dTime : null,
                        // IsDischarge = Convert.ToBoolean(data["discharge"]),
                        IsDischarge = data.TryGetValue("discharge", out var disVal) && bool.TryParse(disVal?.ToString(), out var isDischarge) ? isDischarge : false,
                        FinishOrderTime = TimeOnly.TryParse(data["finishedOrderTime"]?.ToString(), out var fTime) ? fTime : null,
                        IsFinishOrder = Convert.ToBoolean(data["finishedOrder"]),
                        TotalPrice = 0,
                        OrderRemark = data["remark"]?.ToString() ?? "",
                        DiscountID = null,
                        Discount = null,
                        IdInFirestore = doc.Id,
                        TableID = _Table.TableID,
                        Table = _Table ?? new Table() { TableName = tableNumber } // ใช้ค่าเริ่มต้นถ้าไม่พบ
                    };

                    var items = data["items"] as IEnumerable<object>; // ตรวจสอบว่า items มีข้อมูลหรือไม่
                    if (items == null) continue;

                    foreach (var itemObj in items)
                    {
                        var item = itemObj as Dictionary<string, object>;
                        if (item == null) continue;

                        string itemName = item["name"]?.ToString() ?? "";
                        var parts = itemName.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var baseItemName = parts.FirstOrDefault();
                        if (baseItemName == null) continue;

                        var menu = _context.Menus.FirstOrDefault(x => x.Name == baseItemName);
                        if (menu == null)
                        {
                            _logger.LogWarning($"ไม่พบเมนูหลักชื่อ '{baseItemName}' ในฐานข้อมูล");
                            continue;
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
                            Price = Convert.ToDecimal(item["price"]),
                            Toppings = new List<OrderDetailTopping>(),
                            MenuIdInFirestore = item["id"]?.ToString() ?? "",
                            IsDone = _isDone,
                            IsDischarge = _isDischarge,
                            Remark = item["remark"]?.ToString() ?? ""
                        };

                        var toppingNames = parts.Skip(1);
                        foreach (var tName in toppingNames)
                        {
                            var topping = _context.Menus.FirstOrDefault(x => x.Name == tName.Trim());
                            if (topping != null)
                            {
                                _context.Attach(topping);

                                detail.Toppings.Add(new OrderDetailTopping
                                {
                                    MenuId = topping.Id,
                                    Menu = topping,
                                    ToppingPrice = topping.Price
                                });

                                // เพิ่มราคาท้อปปิ้ง
                                //detail.Price += topping.Price;
                            }
                            else
                            {
                                _logger.LogWarning($"Topping ไม่พบในฐานข้อมูล: {tName.Trim()}");
                            }
                        }

                        // คำนวณยอดรวมคำสั่งซื้อ
                        order.TotalPrice += detail.Price * detail.Quantity;

                        _context.OrderDetails.Add(detail);
                    }

                    _context.OrderHeaders.Add(order);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"✅ คัดลอก Order ID: {order.OrderId} | วันที่: {order.OrderDate}, เวลา: {order.OrderTime}");
                    copied++;
                }

            }

            return $"✅ คัดลอกคำสั่งซื้อจาก Firestore แล้วทั้งหมด {copied} รายการ";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ เกิดข้อผิดพลาดขณะคัดลอกคำสั่งซื้อ");
            if (ex.InnerException != null)
            {
                _logger.LogError("🔥 InnerException: " + ex.InnerException.Message);
                Console.WriteLine("🔥 InnerException: " + ex.InnerException.Message);
            }
            return "❌ เกิดข้อผิดพลาด กรุณาตรวจสอบ " + errorFromFirestoreID + " เพิ่มเติม";
        }
    }
    public class ImportOrderExcel
    {
        public string? menu_name { get; set; }
        public string? customer_name { get; set; }
        public string? order_date { get; set; }
        public string? order_time { get; set; }
        public string? finish_order_time { get; set; }
        public string? discharge_time { get; set; }
        public string? discharge_type { get; set; }
        public string? price { get; set; }
        public string? promptpay_price { get; set; }
        public string? cash_price { get; set; }
        public string? unit_price { get; set; }
        public string? qty { get; set; }
        public string? cost { get; set; }
        public string? unit_cost { get; set; }
        public string? profit { get; set; }
    }
    public async Task<string> ImportOrderFromExcel()
    {
        int copied = 0;

        try
        {
            var importData = await _context.Set<ImportOrderExcel>()
                .FromSqlRaw("SELECT * FROM import_orders_excel where order_date ='2025-01-03' ORDER BY order_date, order_time, customer_name")
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

                if (!DateOnly.TryParse(key.date, out var parsedDate))
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

                Ordertype _OrderType = _context.Ordertypes.FirstOrDefault(x => x.OrderTypeName == first.discharge_type) ?? new Ordertype(); // ตรวจสอบว่า OrderType มีอยู่ในฐานข้อมูลหรือไม่
                DischargeType _DischargeType = new DischargeType();
                if (first.cash_price != "0")
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Promptpay");
                }
                else if (first.promptpay_price != "0")
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Cash");
                }
                else if (first.discharge_type == "grab")
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Grab");
                }
                else
                {
                    _DischargeType = _context.DischargeTypes.FirstOrDefault(x => x.DischargeName == "Promptpay");
                }
                 Table _Table = _context.Tables.FirstOrDefault(x => x.TableName == "none") ?? new Table(); // ตรวจสอบว่า Table มีอยู่ในฐานข้อมูลหรือไม่
                 
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
                    OrderRemark =  "",
                    DiscountID = null,
                    Discount = null,
                    IdInFirestore = "",
                    TableID = _Table.TableID,
                    Table = _Table ?? new Table()// ใช้ค่าเริ่มต้นถ้าไม่พบ
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
                       
                    orderHeader.TotalPrice += price * quantity;
                    _context.OrderDetails.Add(orderDetail);
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

    // public async Task<string> ImportOrderFromExcel()
    // {
    //     int copied = 0;

    //     try
    //     {
    //         var importData = await _context.Set<ImportOrderExcel>()
    //             .FromSqlRaw("SELECT * FROM import_orders_excel where order_date ='2025-01-03' ORDER BY order_date, order_time, customer_name")
    //             .ToListAsync();


    //         foreach (var data in importData)
    //         {

    //             var existingOrder = await _context.OrderHeaders.FirstOrDefaultAsync(o =>
    //                 o.CustomerName == data.customer_name &&
    //                 o.OrderDate == TimeOnly.TryParse(first.finish_order_time, out var fTime) ? fTime : null, data.order_date &&
    //                 o.OrderTime == TimeOnly.TryParse(data.order_time, out var fTime) ? fTime : null);

    //             if (existingOrder != null)
    //             {
    //                 _logger.LogInformation($"⚠️ ซ้ำ: {key.customer} {key.date} {key.time}");
    //                 continue;
    //             }

    //             var first = group.First();

    //             var orderHeader = new OrderHeader
    //             {
    //                 CustomerName = key.customer,
    //                 OrderDate = parsedDate,
    //                 OrderTime = parsedTime,
    //                 FinishOrderTime = TimeOnly.TryParse(first.finish_order_time, out var fTime) ? fTime : null,
    //                 DischargeTime = TimeOnly.TryParse(first.discharge_time, out var dTime) ? dTime : null,
    //                 IsDischarge = !string.IsNullOrWhiteSpace(first.discharge_time),
    //                 IsFinishOrder = !string.IsNullOrWhiteSpace(first.finish_order_time),
    //                 DischargeTypeId = 3,
    //                 OrderTypeId = 1, // 🟢 Default
    //                 DiscountID = 0,  // 🟢 Default (nullable)
    //                 TableID = null,  // 🟢 Default
    //                 OrderRemark = string.Empty,
    //                 IdInFirestore = null,
    //                 TotalPrice = 0
    //             };

    //             foreach (var item in group)
    //             {
    //                 if (string.IsNullOrWhiteSpace(item.menu_name)) continue;

    //                 var baseItem = await _context.Menus.FirstOrDefaultAsync(m => m.Name == item.menu_name.Trim());
    //                 if (baseItem == null)
    //                 {
    //                     _logger.LogWarning($"❌ เมนูไม่พบ: {item.menu_name}");
    //                     continue;
    //                 }

    //                 var quantity = int.TryParse(item.qty, out var qtyVal) ? qtyVal : 1;
    //                 var price = decimal.TryParse(item.price, out var priceVal) ? priceVal : baseItem.Price;

    //                 var orderDetail = new OrderDetail
    //                 {
    //                     OrderHeader = orderHeader,
    //                     MenuId = baseItem.Id,
    //                     Quantity = quantity,
    //                     Price = price,
    //                     Menu = baseItem,
    //                     IsDone = false,
    //                     IsDischarge = false,
    //                     Remark = string.Empty,
    //                     MenuIdInFirestore = null
    //                 };

    //                 orderHeader.TotalPrice += price * quantity;
    //                 _context.OrderDetails.Add(orderDetail);
    //             }

    //             _context.OrderHeaders.Add(orderHeader);
    //             await _context.SaveChangesAsync();
    //             copied++;
    //         }

    //         return $"✅ คัดลอกคำสั่งซื้อแล้วทั้งหมด {copied} รายการ";
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "❌ เกิดข้อผิดพลาดระหว่าง import");
    //         if (ex.InnerException != null)
    //         {
    //             _logger.LogError("🔥 InnerException: " + ex.InnerException.Message);
    //         }

    //         return "❌ เกิดข้อผิดพลาด: " + ex.Message;
    //     }
    // }

}