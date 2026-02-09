namespace POS.API.Models;

public class Sale
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; }
    public int TotalAmount { get; set; }
    public int Tax { get; set; } = 0;
    public int Subtotal { get; set; }
    public int Discount { get; set; } = 0;
    public int PaymentAmount { get; set; } = 0;
    public int Balance { get; set; } = 0;
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public PriceType PriceType { get; set; } = PriceType.Retail;
    public bool IsPaid { get; set; } = true;
    public List<SaleItem> Items { get; set; } = new();
    
    public Customer? Customer { get; set; }
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UnitPrice { get; set; }
    public int Total { get; set; }
    
    public Sale? Sale { get; set; }
}

public enum PriceType
{
    Retail = 0,
    Wholesale = 1
}
