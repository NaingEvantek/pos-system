using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.API.Data;
using POS.API.Models;
using POS.API.DTOs;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly POSDbContext _context;

    public CustomersController(POSDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _context.Customers
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var customerDtos = customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email,
            Address = c.Address,
            Type = c.Type.ToString(),
            CurrentDebit = c.CurrentDebit,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            LastPurchase = c.LastPurchase
        }).ToList();

        return Ok(customerDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            Type = customer.Type.ToString(),
            CurrentDebit = customer.CurrentDebit,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            LastPurchase = customer.LastPurchase
        };

        return Ok(customerDto);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createDto)
    {
        var customer = new Customer
        {
            Name = createDto.Name,
            Phone = createDto.Phone,
            Email = createDto.Email,
            Address = createDto.Address,
            Type = (CustomerType)createDto.Type,
            CurrentDebit = 0,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var customerDto = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            Type = customer.Type.ToString(),
            CurrentDebit = customer.CurrentDebit,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt,
            LastPurchase = customer.LastPurchase
        };

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customerDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, CreateCustomerDto updateDto)
    {
        var customer = await _context.Customers.FindAsync(id);
        
        if (customer == null)
        {
            return NotFound();
        }

        customer.Name = updateDto.Name;
        customer.Phone = updateDto.Phone;
        customer.Email = updateDto.Email;
        customer.Address = updateDto.Address;
        customer.Type = (CustomerType)updateDto.Type;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/debit-balance")]
    public async Task<ActionResult<object>> GetDebitBalance(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        
        if (customer == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CurrentDebit = customer.CurrentDebit,
            Type = customer.Type.ToString()
        });
    }

    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<IEnumerable<DebitTransactionDto>>> GetCustomerTransactions(int id)
    {
        var transactions = await _context.DebitTransactions
            .Include(dt => dt.Customer)
            .Where(dt => dt.CustomerId == id)
            .OrderByDescending(dt => dt.TransactionDate)
            .ToListAsync();

        var transactionDtos = transactions.Select(dt => new DebitTransactionDto
        {
            Id = dt.Id,
            CustomerId = dt.CustomerId,
            CustomerName = dt.Customer?.Name ?? "",
            SaleId = dt.SaleId,
            Amount = dt.Amount,
            Type = dt.Type.ToString(),
            Notes = dt.Notes,
            TransactionDate = dt.TransactionDate
        }).ToList();

        return Ok(transactionDtos);
    }

    [HttpPost("debit-transaction")]
    public async Task<ActionResult<DebitTransactionDto>> CreateDebitTransaction(CreateDebitTransactionDto createDto)
    {
        var customer = await _context.Customers.FindAsync(createDto.CustomerId);
        
        if (customer == null)
        {
            return NotFound("Customer not found");
        }

        var transaction = new DebitTransaction
        {
            CustomerId = createDto.CustomerId,
            SaleId = createDto.SaleId,
            Amount = createDto.Amount,
            Type = (TransactionType)createDto.Type,
            Notes = createDto.Notes,
            TransactionDate = DateTime.Now
        };

        // Update customer's current debit (unlimited credit for Royal customers)
        if (transaction.Type == TransactionType.Debit)
        {
            customer.CurrentDebit += transaction.Amount;
        }
        else if (transaction.Type == TransactionType.Credit)
        {
            customer.CurrentDebit -= transaction.Amount;
            if (customer.CurrentDebit < 0) customer.CurrentDebit = 0;
        }
        else if (transaction.Type == TransactionType.Adjustment)
        {
            customer.CurrentDebit = transaction.Amount;
        }

        _context.DebitTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        var transactionDto = new DebitTransactionDto
        {
            Id = transaction.Id,
            CustomerId = transaction.CustomerId,
            CustomerName = customer.Name,
            SaleId = transaction.SaleId,
            Amount = transaction.Amount,
            Type = transaction.Type.ToString(),
            Notes = transaction.Notes,
            TransactionDate = transaction.TransactionDate
        };

        return Ok(transactionDto);
    }

    [HttpGet("royal")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetRoyalCustomers()
    {
        var customers = await _context.Customers
            .Where(c => c.Type == CustomerType.Royal)
            .OrderByDescending(c => c.CurrentDebit)
            .ToListAsync();

        var customerDtos = customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email,
            Address = c.Address,
            Type = c.Type.ToString(),
            CurrentDebit = c.CurrentDebit,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            LastPurchase = c.LastPurchase
        }).ToList();

        return Ok(customerDtos);
    }
}
