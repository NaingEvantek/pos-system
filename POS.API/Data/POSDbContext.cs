using Microsoft.EntityFrameworkCore;
using POS.API.Models;

namespace POS.API.Data;

public class POSDbContext : DbContext
{
    public POSDbContext(DbContextOptions<POSDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<DebitTransaction> DebitTransactions { get; set; }
    public DbSet<InventoryEntry> InventoryEntries { get; set; }
    public DbSet<InventoryEntryItem> InventoryEntryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed initial products with MMK prices (no decimals)
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "Gaming Laptop", Price = 1200000, RetailPrice = 1200000, WholesalePrice = 1000000, Stock = 0, Category = "Electronics" },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless Mouse", Price = 25000, RetailPrice = 25000, WholesalePrice = 20000, Stock = 0, Category = "Electronics" },
            new Product { Id = 3, Name = "Keyboard", Description = "Mechanical Keyboard", Price = 80000, RetailPrice = 80000, WholesalePrice = 65000, Stock = 0, Category = "Electronics" },
            new Product { Id = 4, Name = "Monitor", Description = "27 inch 4K Monitor", Price = 400000, RetailPrice = 400000, WholesalePrice = 350000, Stock = 0, Category = "Electronics" },
            new Product { Id = 5, Name = "Headphones", Description = "Noise Cancelling", Price = 150000, RetailPrice = 150000, WholesalePrice = 120000, Stock = 0, Category = "Electronics" }
        );

        // Configure relationships
        modelBuilder.Entity<Sale>()
            .HasMany(s => s.Items)
            .WithOne(si => si.Sale)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Customer>()
            .HasMany(c => c.DebitTransactions)
            .WithOne(dt => dt.Customer)
            .HasForeignKey(dt => dt.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DebitTransaction>()
            .HasOne(dt => dt.Sale)
            .WithMany()
            .HasForeignKey(dt => dt.SaleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InventoryEntry>()
            .HasMany(ie => ie.Items)
            .WithOne(iei => iei.InventoryEntry)
            .HasForeignKey(iei => iei.InventoryEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryEntryItem>()
            .HasOne(iei => iei.Product)
            .WithMany()
            .HasForeignKey(iei => iei.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
