namespace POS.API.DTOs;

public class InventoryEntryDto
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public int TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<InventoryEntryItemDto> Items { get; set; } = new();
}

public class InventoryEntryItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UnitCost { get; set; }
    public int Total { get; set; }
}

public class CreateInventoryEntryDto
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<CreateInventoryEntryItemDto> Items { get; set; } = new();
}

public class CreateInventoryEntryItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int UnitCost { get; set; }
}
