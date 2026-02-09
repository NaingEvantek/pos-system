namespace POS.API.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public CustomerType Type { get; set; } = CustomerType.WalkIn;
    public int CurrentDebit { get; set; } = 0; // Outstanding balance (unlimited credit for Royal)
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastPurchase { get; set; }
    
    // Navigation properties
    public List<Sale> Sales { get; set; } = new();
    public List<DebitTransaction> DebitTransactions { get; set; } = new();
}

public enum CustomerType
{
    WalkIn = 0,      // No registration required
    Online = 1,      // Online/registered customers
    Royal = 2        // VIP customers with unlimited credit
}

public class DebitTransaction
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? SaleId { get; set; }
    public int Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public Sale? Sale { get; set; }
}

public enum TransactionType
{
    Debit = 0,    // Money owed (purchase on credit)
    Credit = 1,   // Payment received
    Adjustment = 2 // Manual adjustment
}
