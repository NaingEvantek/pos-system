using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.DTOs;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly POSDbContext _context;

    public InventoryController(POSDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryEntryDto>>> GetInventoryEntries()
    {
        var entries = await _context.InventoryEntries
            .Include(ie => ie.Items)
            .ThenInclude(iei => iei.Product)
            .OrderByDescending(ie => ie.EntryDate)
            .ToListAsync();

        var entryDtos = entries.Select(e => new InventoryEntryDto
        {
            Id = e.Id,
            EntryDate = e.EntryDate,
            ReferenceNumber = e.ReferenceNumber,
            Supplier = e.Supplier,
            TotalAmount = e.TotalAmount,
            Notes = e.Notes,
            Items = e.Items.Select(i => new InventoryEntryItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Total = i.Total
            }).ToList()
        }).ToList();

        return Ok(entryDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryEntryDto>> GetInventoryEntry(int id)
    {
        var entry = await _context.InventoryEntries
            .Include(ie => ie.Items)
            .ThenInclude(iei => iei.Product)
            .FirstOrDefaultAsync(ie => ie.Id == id);

        if (entry == null)
        {
            return NotFound();
        }

        var entryDto = new InventoryEntryDto
        {
            Id = entry.Id,
            EntryDate = entry.EntryDate,
            ReferenceNumber = entry.ReferenceNumber,
            Supplier = entry.Supplier,
            TotalAmount = entry.TotalAmount,
            Notes = entry.Notes,
            Items = entry.Items.Select(i => new InventoryEntryItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Total = i.Total
            }).ToList()
        };

        return Ok(entryDto);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryEntryDto>> CreateInventoryEntry(CreateInventoryEntryDto createDto)
    {
        var entry = new InventoryEntry
        {
            EntryDate = DateTime.Now,
            ReferenceNumber = createDto.ReferenceNumber,
            Supplier = createDto.Supplier,
            Notes = createDto.Notes,
            Items = new List<InventoryEntryItem>()
        };

        // Process items and update product stock
        foreach (var itemDto in createDto.Items)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                return BadRequest(new { message = $"Product with ID {itemDto.ProductId} not found" });
            }

            var item = new InventoryEntryItem
            {
                ProductId = itemDto.ProductId,
                ProductName = product.Name,
                Quantity = itemDto.Quantity,
                UnitCost = itemDto.UnitCost,
                Total = itemDto.Quantity * itemDto.UnitCost
            };

            entry.Items.Add(item);

            // Update product stock
            product.Stock += itemDto.Quantity;
        }

        entry.TotalAmount = entry.Items.Sum(i => i.Total);

        _context.InventoryEntries.Add(entry);
        await _context.SaveChangesAsync();

        var entryDto = new InventoryEntryDto
        {
            Id = entry.Id,
            EntryDate = entry.EntryDate,
            ReferenceNumber = entry.ReferenceNumber,
            Supplier = entry.Supplier,
            TotalAmount = entry.TotalAmount,
            Notes = entry.Notes,
            Items = entry.Items.Select(i => new InventoryEntryItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Total = i.Total
            }).ToList()
        };

        return CreatedAtAction(nameof(GetInventoryEntry), new { id = entry.Id }, entryDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInventoryEntry(int id)
    {
        var entry = await _context.InventoryEntries
            .Include(ie => ie.Items)
            .FirstOrDefaultAsync(ie => ie.Id == id);

        if (entry == null)
        {
            return NotFound();
        }

        // Reverse stock changes
        foreach (var item in entry.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
            }
        }

        _context.InventoryEntries.Remove(entry);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
