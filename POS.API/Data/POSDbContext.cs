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
            new Product { Id = 1, Name = "ကော်ဂျူလှိုင်း", Description = "အပြာရောင်", Price = 12000, RetailPrice = 12000, WholesalePrice = 11000, Stock = 100, Category = "ဂါဝန်" }

        );

        modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        Id = 1,
                        Username = "admin",
                        Email = "admin@pos.com",
                        PasswordHash = "$2a$12$P2UjP6sSt5IURLIIl3NrMONs4Nxo7XYdi2uTy4loHEiiO0Y408fja", // admin123
                        FullName = "System Administrator",
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        Id = 2,
                        Username = "cashier",
                        Email = "cashier@pos.com",
                        PasswordHash = "$2a$12$P2UjP6sSt5IURLIIl3NrMONs4Nxo7XYdi2uTy4loHEiiO0Y408fja", // admin123
                        FullName = "Default Cashier",
                        Role = UserRole.Cashier,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
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
