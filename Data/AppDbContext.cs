using Microsoft.EntityFrameworkCore;

namespace FactoryOrders.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ManufacturingOrder> Orders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ManufacturingOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.ItemName).HasMaxLength(200);
            entity.Property(e => e.OrderedBy).HasMaxLength(100);
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.MachineOrLine).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });
    }
}
