namespace POS.PrintService.Models;

public class ReceiptModel
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal Balance { get; set; }
    public List<ReceiptItem> Items { get; set; } = new();
}

public class ReceiptItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}
