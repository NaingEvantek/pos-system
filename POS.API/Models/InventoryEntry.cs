namespace POS.API.Models;

public class InventoryEntry
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Now;
    public string ReferenceNumber { get; set; } = string.Empty; // Invoice/PO number
    public string Supplier { get; set; } = string.Empty;
    public int TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<InventoryEntryItem> Items { get; set; } = new();
}

public class InventoryEntryItem
{
    public int Id { get; set; }
    public int InventoryEntryId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int UnitCost { get; set; }
    public int Total { get; set; }
    
    public InventoryEntry? InventoryEntry { get; set; }
    public Product? Product { get; set; }
}
