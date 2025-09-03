public class DailySaleDto
{
    public DateOnly SaleDate { get; set; }   // วันที่ขาย
    public int Orders { get; set; }          // จำนวนบิล
    public decimal TotalAmount { get; set; } // ยอดขายรวม
    public double AvgPerOrder { get; set; }  // ค่าเฉลี่ยต่อบิล
}
