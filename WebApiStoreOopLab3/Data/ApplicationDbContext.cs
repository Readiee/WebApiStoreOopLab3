using Microsoft.EntityFrameworkCore;
using WebApiStoreOopLab3.Models;

namespace WebApiStoreOopLab3.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Store> Stores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<InventoryItem> Inventory { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>().HasKey(s => s.Code);
        modelBuilder.Entity<Product>().HasKey(p => p.Name);

        modelBuilder.Entity<InventoryItem>()
            .HasKey(i => new { i.StoreCode, i.ProductName });

        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Store)
            .WithMany()
            .HasForeignKey(i => i.StoreCode);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductName);
    }
}
