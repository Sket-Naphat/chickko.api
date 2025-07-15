using chickko.api.Data;
using chickko.api.Interface;
using chickko.api.Models;
using chickko.api.Services;
using Google.Cloud.Firestore;
public class OrdersService : IOrdersService
{
    private readonly ChickkoContext _context;
    private readonly ILogger<OrdersService> _logger;
    private readonly IUtilService _utilService;

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
        try
        {
            var snapshot = await _utilService.GetSnapshotFromFirestoreWithFiltersBetween("orders", "orderDate", datefrom, dateto);

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                string dischargeName = data["dischargeType"]?.ToString() ?? "";
                string locName = data["locationOrder"]?.ToString() ?? "";
                string tableNumber = data["tableNumber"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(dischargeName) || string.IsNullOrEmpty(locName))
                {
                    _logger.LogWarning($"ข้อมูลไม่ครบสำหรับ Order ID: {doc.Id}");
                    continue;
                }

                var order = new OrderHeader
                {
                    CustomerName = data["customerName"]?.ToString() ?? "",
                    OrderDate = DateOnly.TryParse(data["orderDate"]?.ToString(), out var orderDate) ? orderDate : null,
                    OrderTime = TimeOnly.TryParse(data["orderTime"]?.ToString(), out var orderTime) ? orderTime : null,
                    OrderTypeId = _context.Ordertypes.First(x => x.OrderTypeName == locName).OrderTypeId,
                    OrderType = _context.Ordertypes.First(x => x.OrderTypeName == locName),
                    DischargeTypeId = _context.DischargeTypes.First(x => x.DischargeName == dischargeName).DischargeTypeId,
                    DischargeType = _context.DischargeTypes.First(x => x.DischargeName == dischargeName),
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
                    TableID = _context.Tables.FirstOrDefault(x => x.TableName == tableNumber)?.TableID,
                    Table = _context.Tables.FirstOrDefault(x => x.TableName == tableNumber)
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
                        //IsDone = Convert.ToBoolean(item["is_done"]),
                        IsDone = data.TryGetValue("is_done", out var itemis_done) && bool.TryParse(itemis_done?.ToString(), out var isItemIsDone) ? isItemIsDone : false,                     
                        //IsDischarge = Convert.ToBoolean(item["itemDischarge"]),
                        IsDischarge = data.TryGetValue("itemDischarge", out var itemDisVal) && bool.TryParse(itemDisVal?.ToString(), out var isItemDischarge) ? isItemDischarge : false,
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
            return "❌ เกิดข้อผิดพลาด กรุณาตรวจสอบ log เพิ่มเติม";
        }
    }

}