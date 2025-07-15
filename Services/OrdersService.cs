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
                var data = doc.ToDictionary();
                if (copied == 62)
                {
                    _logger.LogWarning("เกินจำนวนที่กำหนดไว้ 60 รายการ");
                }

                // string dischargeName = data["dischargeType"]?.ToString().Trim() ?? "Promptpay";
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

}