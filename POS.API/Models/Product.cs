namespace POS.API.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RetailPrice { get; set; }
    public int WholesalePrice { get; set; }
    public int Price { get; set; } // Kept for backward compatibility
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
}
