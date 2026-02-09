using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.DTOs;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly POSDbContext _context;

    public SalesController(POSDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetSales()
    {
        var sales = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

        var salesDto = sales.Select(s => new SaleDto
        {
            Id = s.Id,
            SaleDate = s.SaleDate,
            TotalAmount = s.TotalAmount,
            Tax = s.Tax,
            Subtotal = s.Subtotal,
            Discount = s.Discount,
            PaymentAmount = s.PaymentAmount,
            Balance = s.Balance,
            PaymentMethod = s.PaymentMethod,
            CustomerName = s.CustomerName,
            CustomerId = s.CustomerId,
            PriceType = s.PriceType.ToString(),
            IsPaid = s.IsPaid,
            Items = s.Items.Select(i => new SaleItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList()
        }).ToList();

        return Ok(salesDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDto>> GetSale(int id)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null)
        {
            return NotFound();
        }

        var saleDto = new SaleDto
        {
            Id = sale.Id,
            SaleDate = sale.SaleDate,
            TotalAmount = sale.TotalAmount,
            Tax = sale.Tax,
            Subtotal = sale.Subtotal,
            Discount = sale.Discount,
            PaymentAmount = sale.PaymentAmount,
            Balance = sale.Balance,
            PaymentMethod = sale.PaymentMethod,
            CustomerName = sale.CustomerName,
            CustomerId = sale.CustomerId,
            PriceType = sale.PriceType.ToString(),
            IsPaid = sale.IsPaid,
            Items = sale.Items.Select(i => new SaleItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList()
        };

        return Ok(saleDto);
    }

    [HttpPost]
    public async Task<ActionResult<SaleDto>> CreateSale(CreateSaleDto createSaleDto)
    {
        Customer? customer = null;

        // Find or create customer based on phone number
        if (!string.IsNullOrWhiteSpace(createSaleDto.CustomerPhone))
        {
            // Try to find existing customer by phone
            customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Phone == createSaleDto.CustomerPhone);

            if (customer != null)
            {
                // Update existing customer info
                customer.Name = createSaleDto.CustomerName;
                customer.Address = createSaleDto.CustomerAddress ?? customer.Address;
                customer.LastPurchase = DateTime.Now;
            }
            else
            {
                // Create new customer
                customer = new Customer
                {
                    Name = createSaleDto.CustomerName,
                    Phone = createSaleDto.CustomerPhone,
                    Address = createSaleDto.CustomerAddress ?? string.Empty,
                    Email = string.Empty,
                    Type = CustomerType.WalkIn, // Default to WalkIn
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    LastPurchase = DateTime.Now
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync(); // Save to get ID
            }
        }
        else if (createSaleDto.CustomerId.HasValue)
        {
            // Use existing customer ID (for Royal customers)
            customer = await _context.Customers.FindAsync(createSaleDto.CustomerId.Value);
            if (customer == null)
            {
                return BadRequest(new { message = $"Customer with ID {createSaleDto.CustomerId.Value} not found" });
            }
            customer.LastPurchase = DateTime.Now;
        }

        var sale = new Sale
        {
            CustomerName = createSaleDto.CustomerName,
            CustomerId = customer?.Id,
            PaymentMethod = createSaleDto.PaymentMethod,
            PriceType = (PriceType)createSaleDto.PriceType,
            SaleDate = DateTime.Now,
            Discount = createSaleDto.Discount,
            PaymentAmount = createSaleDto.PaymentAmount,
            Balance = createSaleDto.Balance,
            IsPaid = createSaleDto.IsPaid,
            Items = createSaleDto.Items.Select(i => new SaleItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList()
        };
        
        // Calculate totals (tax removed as per requirement)
        sale.Subtotal = sale.Items.Sum(i => i.Total);
        sale.Tax = 0; // Tax removed
        sale.TotalAmount = sale.Subtotal - sale.Discount;

        // Update product stock
        foreach (var item in sale.Items)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Name == item.ProductName);
            
            if (product != null)
            {
                product.Stock -= item.Quantity;
            }
        }

        // Handle Royal customer credit (unlimited)
        if (customer != null && customer.Type == CustomerType.Royal && !sale.IsPaid)
        {
            // Add to customer's debit (no credit limit check - unlimited)
            customer.CurrentDebit += sale.TotalAmount;

            // Create debit transaction
            var debitTransaction = new DebitTransaction
            {
                CustomerId = customer.Id,
                SaleId = sale.Id,
                Amount = sale.TotalAmount,
                Type = TransactionType.Debit,
                Notes = $"Sale #{sale.Id}",
                TransactionDate = DateTime.Now
            };
            _context.DebitTransactions.Add(debitTransaction);
        }

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var saleDto = new SaleDto
        {
            Id = sale.Id,
            SaleDate = sale.SaleDate,
            TotalAmount = sale.TotalAmount,
            Tax = sale.Tax,
            Subtotal = sale.Subtotal,
            Discount = sale.Discount,
            PaymentAmount = sale.PaymentAmount,
            Balance = sale.Balance,
            PaymentMethod = sale.PaymentMethod,
            CustomerName = sale.CustomerName,
            CustomerId = sale.CustomerId,
            PriceType = sale.PriceType.ToString(),
            IsPaid = sale.IsPaid,
            Items = sale.Items.Select(i => new SaleItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList()
        };

        return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, saleDto);
    }

    [HttpGet("today")]
    public async Task<ActionResult<object>> GetTodaySales()
    {
        var today = DateTime.Today;
        var sales = await _context.Sales
            .Where(s => s.SaleDate >= today)
            .Include(s => s.Items)
            .ToListAsync();

        var salesDto = sales.Select(s => new SaleDto
        {
            Id = s.Id,
            SaleDate = s.SaleDate,
            TotalAmount = s.TotalAmount,
            Tax = s.Tax,
            Subtotal = s.Subtotal,
            PaymentMethod = s.PaymentMethod,
            CustomerName = s.CustomerName,
            Items = s.Items.Select(i => new SaleItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList()
        }).ToList();

        return Ok(new
        {
            TotalSales = salesDto.Count,
            TotalRevenue = salesDto.Sum(s => s.TotalAmount),
            Sales = salesDto
        });
    }
}
