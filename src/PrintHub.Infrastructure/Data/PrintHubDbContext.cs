using Microsoft.EntityFrameworkCore;
using PrintHub.Core.Entities;

namespace PrintHub.Infrastructure.Data;

public class PrintHubDbContext : DbContext
{
    public PrintHubDbContext(DbContextOptions<PrintHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
    public DbSet<PrintJobItem> PrintJobItems => Set<PrintJobItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToContainer("Users").HasPartitionKey(u => u.Id);
        modelBuilder.Entity<Shop>().ToContainer("Shops").HasPartitionKey(s => s.Id);
        modelBuilder.Entity<Product>().ToContainer("Products").HasPartitionKey(p => p.Id);
        modelBuilder.Entity<Part>().ToContainer("Parts").HasPartitionKey(p => p.Id);
        modelBuilder.Entity<PrintJob>().ToContainer("PrintJobs").HasPartitionKey(j => j.Id);
        modelBuilder.Entity<PrintJobItem>().ToContainer("PrintJobItems").HasPartitionKey(i => i.Id);
    }
}
