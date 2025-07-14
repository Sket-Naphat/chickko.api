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
        int copied = 0; // ตัวแปรนับจำนวนที่คัดลอกได้
        try
        {
            // ดึง snapshot (ข้อมูล) จาก Firestore ที่อยู่ระหว่างวันที่ที่กำหนด
            var snapshot = await _utilService.GetSnapshotFromFirestoreWithFiltersBetween("orders", "orderDate", datefrom, dateto);

            // วนลูปข้อมูลแต่ละคำสั่งซื้อใน Firestore
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary(); // แปลงข้อมูลแต่ละ document เป็น Dictionary

                // อ่านชื่อ dischargeType, สถานที่, และหมายเลขโต๊ะ จาก Firestore
                string dischargeName = data["dischargeType"]?.ToString() ?? "";
                string locName = data["locationOrder"]?.ToString() ?? "";
                string tableNumber = data["tableNumber"]?.ToString() ?? "";

                // ถ้าข้อมูลไม่ครบ ให้ข้ามไป
                if (string.IsNullOrEmpty(dischargeName) || string.IsNullOrEmpty(locName))
                {
                    _logger.LogWarning($"ข้อมูลไม่ครบถ้วนสำหรับคำสั่งซื้อ ID: {doc.Id}");
                    continue;
                }

                // สร้าง OrderHeader ใหม่จากข้อมูลใน Firestore
                var order = new OrderHeader
                {
                    CustomerName = data["customerName"]?.ToString() ?? "",
                    IsDischarge = Convert.ToBoolean(data["discharge"]),
                    DischargeTime = TimeOnly.TryParse(data["dischargeTime"]?.ToString(), out var dTime) ? dTime : (TimeOnly?)null,
                    IsFinishOrder = Convert.ToBoolean(data["finishedOrder"]),
                    FinishOrderTime = TimeOnly.TryParse(data["finishedOrderTime"]?.ToString(), out var fTime) ? fTime : (TimeOnly?)null,
                    OrderDate = DateOnly.TryParse(data["orderDate"]?.ToString(), out var orderDate) ? orderDate : (DateOnly?)null,
                    OrderTime = TimeOnly.TryParse(data["orderTime"]?.ToString(), out var orderTime) ? orderTime : (TimeOnly?)null,
                    OrderRemark = data["remark"]?.ToString() ?? "",
                    TotalPrice = 0,
                    IdInFirestore = doc.Id,
                    DischargeTypeId = _context.DischargeTypes.First(x => x.DischargeName == dischargeName).DischargeTypeId,
                    DischargeType = _context.DischargeTypes.First(x => x.DischargeName == dischargeName),
                    OrderTypeId = _context.Ordertypes.First(x => x.OrderTypeName == locName).OrderTypeId,
                    OrderType = _context.Ordertypes.First(x => x.OrderTypeName == locName),
                    TableID = _context.Tables.FirstOrDefault(x => x.TableName == tableNumber)?.TableID,
                    Table = _context.Tables.FirstOrDefault(x => x.TableName == tableNumber),
                    DiscountID = 0
                };

                var items = data["items"] as IEnumerable<object>;
                if (items == null) continue;

                foreach (var itemObj in items)
                {
                    var item = itemObj as Dictionary<string, object>;
                    if (item == null) continue;

                    string itemName = item["name"]?.ToString() ?? "";
                    var baseItemName = itemName.Split('+', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                    var menu = _context.Menus.FirstOrDefault(x => x.Name == baseItemName);
                    if (menu == null)
                    {
                        _logger.LogWarning($"ไม่พบเมนูที่ชื่อ {baseItemName} ในฐานข้อมูล");
                        continue;
                    }

                    var toppingNames = itemName.Split('+', StringSplitOptions.RemoveEmptyEntries).Skip(1);
                    var toppings = new List<OrderDetailTopping>();

                    foreach (var name in toppingNames)
                    {
                        var cleanName = name.Trim();
                        var toppingMenu = _context.Menus.FirstOrDefault(x => x.Name == cleanName);
                        if (toppingMenu != null)
                        {
                            _context.Attach(toppingMenu);
                            toppings.Add(new OrderDetailTopping
                            {
                                MenuId = toppingMenu.Id,
                                Menu = toppingMenu
                            });
                        }
                    }

                    var detail = new OrderDetail
                    {
                        IsDone = Convert.ToBoolean(item["is_done"]),
                        MenuIdInFirestore = item["id"]?.ToString() ?? "",
                        OrderHeader = order,
                        OrderId = order.OrderId,
                        Menu = menu,
                        MenuId = menu.Id,
                        IsDischarge = Convert.ToBoolean(item["itemDischarge"]),
                        Price = Convert.ToDecimal(item["price"]),
                        Quantity = Convert.ToInt32(item["quantity"]),
                        Remark = item["remark"]?.ToString() ?? "",
                        Toppings = toppings
                    };

                    _context.OrderDetails.Add(detail);
                    order.TotalPrice += detail.Price * detail.Quantity;
                }

                _context.OrderHeaders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"คัดลอกคำสั่งซื้อ ID: {order.OrderId} วันที่: {order.OrderDate}, เวลา: {order.OrderTime}");
                copied++;
            }

            return $"คัดลอกคำสั่งซื้อจาก Firestore มา {copied} รายการเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกคำสั่งซื้อจาก Firestore");
            if (ex.InnerException != null)
            {
                _logger.LogError("🔥 รายละเอียดเพิ่มเติมจาก InnerException: " + ex.InnerException.Message);
                Console.WriteLine("🔥 InnerException: " + ex.InnerException.Message);
            }
            return "เกิดข้อผิดพลาดในการคัดลอกคำสั่งซื้อ กรุณาตรวจสอบ log เพิ่มเติม";
        }
    }
}