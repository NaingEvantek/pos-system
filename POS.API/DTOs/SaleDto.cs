namespace POS.API.DTOs;

public class SaleDto
{
    public int Id { get; set; }
    public DateTime SaleDate { get; set; }
    public int TotalAmount { get; set; }
    public int Tax { get; set; }
    public int Subtotal { get; set; }
    public int Discount { get; set; }
    public int PaymentAmount { get; set; }
    public int Balance { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string PriceType { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public List<SaleItemDto> Items { get; set; } = new();
}

public class SaleItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UnitPrice { get; set; }
    public int Total { get; set; }
}

public class CreateSaleDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int PriceType { get; set; } = 0; // 0=Retail, 1=Wholesale
    public int Discount { get; set; } = 0;
    public int PaymentAmount { get; set; } = 0;
    public int Balance { get; set; } = 0;
    public bool IsPaid { get; set; } = true;
    public List<CreateSaleItemDto> Items { get; set; } = new();
}

public class CreateSaleItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UnitPrice { get; set; }
    public int Total { get; set; }
}
