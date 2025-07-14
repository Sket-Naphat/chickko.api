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
        try
        {
            var snapshot = await _utilService.GetSnapshotFromFirestoreWithFiltersBetween("orders", "orderDate", datefrom, dateto); // สมมติว่าคุณมี Firestore SDK setup แล้ว
            int copied = 0;

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();// แปลงข้อมูลจาก Firestore เป็น Dictionary ชนิด Dictionary<string, object> คือการแปลงข้อมูลที่ได้จาก Firestore ให้เป็นรูปแบบที่ใช้งานได้ใน C#

                // 1. Map ข้อมูลหลักจาก Firestore
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
                    TotalPrice = 0  // จะคำนวณทีหลัง
                };

                // 2. Map foreign key โดยหาจากชื่อ

                string dischargeName = data["dischargeType"]?.ToString() ?? "";
                string locName = data["locationOrder"]?.ToString() ?? "";
                string tableNumber = data["tableNumber"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(dischargeName) || string.IsNullOrEmpty(locName))
                {
                    _logger.LogWarning($"ข้อมูลไม่ครบถ้วนสำหรับคำสั่งซื้อ ID: {doc.Id} วันที่: {order.OrderDate}, เวลา: {order.OrderTime}");
                    continue; // ข้ามคำสั่งซื้อนี้หากข้อมูลไม่ครบ
                }
                order.DischargeTypeId = _context.DischargeTypes.First(x => x.DischargeName == dischargeName).DischargeTypeId;
                order.OrderTypeId = _context.Ordertypes.First(x => x.OrderTypeName == locName).OrderTypeId;
                order.TableID = _context.Tables.FirstOrDefault(x => x.TableName == tableNumber)?.TableID;

                // 3. บันทึก OrderHeader ก่อน เพื่อให้ได้ OrderId
                _context.OrderHeaders.Add(order);
                // await _context.SaveChangesAsync();

                // 4. Map รายการ items
                var items = data["items"] as IEnumerable<object>;

                foreach (var itemObj in items!)
                {
                    var item = itemObj as Dictionary<string, object>;
                    if (item == null)
                    {
                        continue; // Skip if item is null
                    }

                    string itemName = item["name"]?.ToString() ?? "";
                    int menuId = _context.Menus.First(x => x.Name == itemName).Id;

                    var detail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        MenuId = menuId,
                        Quantity = Convert.ToInt32(item["quantity"]),
                        Price = Convert.ToDecimal(item["price"]),
                        IsDone = Convert.ToBoolean(item["is_done"]),
                        IsDischarge = Convert.ToBoolean(item["itemDischarge"]),
                        Remark = item["remark"]?.ToString() ?? ""
                    };

                    _context.OrderDetails.Add(detail);
                    order.TotalPrice += detail.Price * detail.Quantity;
                }

                // 5. อัปเดตราคารวม
                // _context.OrderHeaders.Update(order);
                // await _context.SaveChangesAsync();
                copied++;
            }

            return $"คัดลอกคำสั่งซื้อจาก Firestore มา {copied} รายการเรียบร้อยแล้ว";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิดข้อผิดพลาดในการคัดลอกคำสั่งซื้อจาก Firestore");
            return "เกิดข้อผิดพลาดในการคัดลอกคำสั่งซื้อ";
        }
    }

    public Task<string> CopyOrderFromFirestore()
    {
        throw new NotImplementedException();
    }
}