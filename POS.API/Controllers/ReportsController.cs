using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly POSDbContext _context;

    public ReportsController(POSDbContext context)
    {
        _context = context;
    }

    [HttpGet("sales-summary")]
    public async Task<ActionResult<object>> GetSalesSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.Today;
        var end = endDate ?? DateTime.Today.AddDays(1);

        var sales = await _context.Sales
            .Where(s => s.SaleDate >= start && s.SaleDate < end)
            .Include(s => s.Items)
            .ToListAsync();

        return Ok(new
        {
            Period = new { Start = start, End = end },
            TotalSales = sales.Count,
            TotalRevenue = sales.Sum(s => s.TotalAmount),
            TotalTax = sales.Sum(s => s.Tax),
            AverageSale = sales.Any() ? sales.Average(s => s.TotalAmount) : 0,
            PaymentMethods = sales.GroupBy(s => s.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count(), Total = g.Sum(s => s.TotalAmount) })
                .ToList(),
            TopProducts = sales.SelectMany(s => s.Items)
                .GroupBy(i => i.ProductName)
                .Select(g => new { Product = g.Key, Quantity = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.Total) })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList()
        });
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<object>> GetInventoryReport()
    {
        var products = await _context.Products.ToListAsync();

        return Ok(new
        {
            TotalProducts = products.Count,
            TotalValue = products.Sum(p => p.RetailPrice * p.Stock),
            LowStockItems = products.Where(p => p.Stock < 10).Select(p => new
            {
                p.Id,
                p.Name,
                p.Stock,
                p.RetailPrice
            }).ToList(),
            OutOfStockItems = products.Where(p => p.Stock == 0).Select(p => new
            {
                p.Id,
                p.Name
            }).ToList(),
            ByCategory = products.GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(p => p.RetailPrice * p.Stock)
                })
                .ToList()
        });
    }

    [HttpGet("customer-debits")]
    public async Task<ActionResult<object>> GetCustomerDebitReport()
    {
        var royalCustomers = await _context.Customers
            .Where(c => c.Type == CustomerType.Royal)
            .ToListAsync();

        var totalDebit = royalCustomers.Sum(c => c.CurrentDebit);
        var customersWithDebit = royalCustomers.Where(c => c.CurrentDebit > 0).ToList();

        return Ok(new
        {
            TotalRoyalCustomers = royalCustomers.Count,
            CustomersWithDebit = customersWithDebit.Count,
            TotalOutstandingDebit = totalDebit,
            Customers = customersWithDebit.Select(c => new
            {
                c.Id,
                c.Name,
                c.Phone,
                c.CurrentDebit
            }).OrderByDescending(c => c.CurrentDebit).ToList()
        });
    }

    [HttpGet("daily-sales")]
    public async Task<ActionResult<object>> GetDailySalesReport([FromQuery] int days = 7)
    {
        var startDate = DateTime.Today.AddDays(-days);
        
        var sales = await _context.Sales
            .Where(s => s.SaleDate >= startDate)
            .ToListAsync();

        var dailyData = sales
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                SalesCount = g.Count(),
                Revenue = g.Sum(s => s.TotalAmount),
                Tax = g.Sum(s => s.Tax)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return Ok(new
        {
            Period = $"Last {days} days",
            DailyBreakdown = dailyData,
            TotalRevenue = dailyData.Sum(d => d.Revenue),
            TotalSales = dailyData.Sum(d => d.SalesCount),
            AverageDailyRevenue = dailyData.Any() ? dailyData.Average(d => d.Revenue) : 0
        });
    }

    [HttpGet("customer-sales")]
    public async Task<ActionResult<object>> GetCustomerSalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today.AddDays(1);

        var sales = await _context.Sales
            .Include(s => s.Customer)
            .Where(s => s.SaleDate >= start && s.SaleDate < end && s.CustomerId != null)
            .ToListAsync();

        var customerSales = sales
            .GroupBy(s => new { s.CustomerId, CustomerName = s.Customer!.Name, CustomerType = s.Customer.Type })
            .Select(g => new
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                CustomerType = g.Key.CustomerType.ToString(),
                TotalPurchases = g.Count(),
                TotalSpent = g.Sum(s => s.TotalAmount),
                AveragePurchase = g.Average(s => s.TotalAmount),
                LastPurchase = g.Max(s => s.SaleDate)
            })
            .OrderByDescending(c => c.TotalSpent)
            .ToList();

        return Ok(new
        {
            Period = new { Start = start, End = end },
            TotalCustomers = customerSales.Count,
            TotalRevenue = customerSales.Sum(c => c.TotalSpent),
            TopCustomers = customerSales.Take(10),
            ByCustomerType = customerSales.GroupBy(c => c.CustomerType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(c => c.TotalSpent)
                })
                .ToList()
        });
    }

    [HttpGet("price-type-analysis")]
    public async Task<ActionResult<object>> GetPriceTypeAnalysis([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.Today.AddMonths(-1);
        var end = endDate ?? DateTime.Today.AddDays(1);

        var sales = await _context.Sales
            .Where(s => s.SaleDate >= start && s.SaleDate < end)
            .ToListAsync();

        var analysis = sales
            .GroupBy(s => s.PriceType)
            .Select(g => new
            {
                PriceType = g.Key.ToString(),
                SalesCount = g.Count(),
                TotalRevenue = g.Sum(s => s.TotalAmount),
                AverageSale = g.Any() ? (int)g.Average(s => s.TotalAmount) : 0
            })
            .ToList();

        return Ok(new
        {
            Period = new { Start = start, End = end },
            Analysis = analysis,
            RetailPercentage = sales.Any() ? (sales.Count(s => s.PriceType == PriceType.Retail) * 100.0 / sales.Count) : 0,
            WholesalePercentage = sales.Any() ? (sales.Count(s => s.PriceType == PriceType.Wholesale) * 100.0 / sales.Count) : 0
        });
    }

    [HttpGet("monthly-sales-chart")]
    public async Task<ActionResult<object>> GetMonthlySalesChart()
    {
        var today = DateTime.Today;
        var startDate = today.AddMonths(-11).AddDays(-today.Day + 1); // First day of 11 months ago
        
        var sales = await _context.Sales
            .Where(s => s.SaleDate >= startDate)
            .ToListAsync();

        // Group by year-month
        var monthlySales = new List<object>();
        
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = today.AddMonths(-i).AddDays(-today.Day + 1);
            var monthEnd = monthStart.AddMonths(1);
            
            var monthSales = sales
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEnd)
                .ToList();

            monthlySales.Add(new
            {
                Month = monthStart.ToString("MMM yyyy"),
                Year = monthStart.Year,
                MonthNumber = monthStart.Month,
                TotalRevenue = monthSales.Sum(s => s.TotalAmount),
                TotalSales = monthSales.Count,
                RetailRevenue = monthSales.Where(s => s.PriceType == PriceType.Retail).Sum(s => s.TotalAmount),
                WholesaleRevenue = monthSales.Where(s => s.PriceType == PriceType.Wholesale).Sum(s => s.TotalAmount),
                RetailCount = monthSales.Count(s => s.PriceType == PriceType.Retail),
                WholesaleCount = monthSales.Count(s => s.PriceType == PriceType.Wholesale)
            });
        }

        return Ok(new
        {
            Period = "Last 12 Months",
            StartDate = startDate,
            EndDate = today,
            MonthlySales = monthlySales,
            TotalRevenue = monthlySales.Sum(m => (int)((dynamic)m).TotalRevenue),
            TotalSales = monthlySales.Sum(m => (int)((dynamic)m).TotalSales)
        });
    }
}
