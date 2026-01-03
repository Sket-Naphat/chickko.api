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
        public string IncomeDescription { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateOnly? UpdateDate { get; set; }
        public TimeOnly? UpdateTime { get; set; }
    }
}