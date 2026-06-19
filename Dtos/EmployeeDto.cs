namespace chickko.api.Dtos
{
    public class UpdateEmployeeDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public string Username { get; set; } = "";
        public string Contact { get; set; } = "";
        public string Site { get; set; } = "";
        public int? BankID { get; set; }
        public string? BankAccount { get; set; }
        public double WageCost { get; set; }
        public int UserPermistionID { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmployeeDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string Contact { get; set; } = "";
        public bool IsActive { get; set; }
        public string Site { get; set; } = "";
        public DateTime StartWorkDate { get; set; }
        public int UserPermistionID { get; set; }
        public string UserPermistionName { get; set; } = "";
        public double WageCost { get; set; }
        public int? BankID { get; set; }
        public string? BankName { get; set; }
        public string? BankAccount { get; set; }
    }
}
