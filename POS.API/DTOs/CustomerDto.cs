namespace POS.API.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int CurrentDebit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastPurchase { get; set; }
}

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Type { get; set; } = 0; // 0=WalkIn, 1=Online, 2=Royal
}

public class DebitTransactionDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? SaleId { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}

public class CreateDebitTransactionDto
{
    public int CustomerId { get; set; }
    public int? SaleId { get; set; }
    public int Amount { get; set; }
    public int Type { get; set; } // 0=Debit, 1=Credit, 2=Adjustment
    public string Notes { get; set; } = string.Empty;
}
