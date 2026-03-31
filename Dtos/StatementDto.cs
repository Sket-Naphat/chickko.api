namespace chickko.api.Dtos
{
    public class StatementDto
    {
        public int StatementId { get; set; }
        public DateOnly StatementDate { get; set; }
        public TimeOnly StatementTime { get; set; }
        public decimal StatementValue { get; set; }
        public string StatementDescription { get; set; } = string.Empty;
        public int UserId { get; set; }
        public bool Active { get; set; } = true;
        public DateOnly? UpdateDate { get; set; }
        public TimeOnly? UpdateTime { get; set; }
    }
    public class IncomeStatementDto
    {
        public int IncomeId { get; set; }
        public DateOnly IncomeDate { get; set; }
        public TimeOnly IncomeTime { get; set; }
        public decimal IncomeValue { get; set; }
        public int IncomeTypeId { get; set; }
        public string IncomeTypeName { get; set; } = string.Empty;
        public string IncomeDescription { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateOnly? UpdateDate { get; set; }
        public TimeOnly? UpdateTime { get; set; }
    }

    public class StatementFilterDto
    {
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }

    // DTO สำหรับข้อมูลรวม
    public class DailyReportSummaryDto
    {
        public decimal Balance { get; set; }                    // ยอดเงินคงเหลือ
        public decimal TotalBankAccount { get; set; }           // เงินบัญชีธนาคารรวม
        public decimal TotalCash { get; set; }                  // เงินสดรวม
        public decimal TotalCost { get; set; }                  // ต้นทุนรวม
        public decimal TotalTransferCost { get; set; }          // ต้นทุนเงินโอนรวม
        public decimal TotalCashCost { get; set; }              // ต้นทุนเงินสดรวม
        public decimal TotalIncome { get; set; }                // รายได้รวม
        public decimal NetProfit { get; set; }                  // กำไร net
        public decimal HiddenCost { get; set; }                 // ต้นทุนแฝง

        public List<DailyStatementDto> DailyStatements { get; set; } = new();
    }

    // DTO สำหรับแต่ละวัน
    public class DailyStatementDto
    {
        public DateOnly Date { get; set; }
        public decimal TotalCost { get; set; }                  // ต้นทุนรวม
        public decimal TransferCost { get; set; }               // ต้นทุนเงินโอนรวม
        public decimal CashCost { get; set; }                   // ต้นทุนเงินสดรวม
        public decimal TotalIncome { get; set; }                // ยอดเงินเข้า
        public decimal BankIncome { get; set; }                 // ยอดเงินบัญชีธนาคารเข้ารวม
        public decimal CashIncome { get; set; }                 // ยอดเงินสดเข้ารวม
        public decimal Balance { get; set; }                    // ยอดเงินคงเหลือ
        public decimal Sales { get; set; }                      // ยอดขาย
        public decimal Profit { get; set; }                     // กำไร
        public decimal Difference { get; set; }                 // ส่วนต่างเงินเข้ากับยอดขาย
    }
}