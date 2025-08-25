using Microsoft.EntityFrameworkCore;
using software_estimator.Models;

namespace software_estimator.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Estimate> Estimates => Set<Estimate>();
    public DbSet<FunctionalLineItem> FunctionalLineItems => Set<FunctionalLineItem>();
    public DbSet<NonFunctionalItem> NonFunctionalItems => Set<NonFunctionalItem>();
    public DbSet<ResourceAllocation> ResourceAllocations => Set<ResourceAllocation>();
    public DbSet<ResourceRate> ResourceRates => Set<ResourceRate>();
    public DbSet<RoleMapping> RoleMappings => Set<RoleMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Estimate>()
            .HasMany(e => e.FunctionalItems)
            .WithOne(fi => fi.Estimate!)
            .HasForeignKey(fi => fi.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Estimate>()
            .HasMany(e => e.NonFunctionalItems)
            .WithOne(nf => nf.Estimate!)
            .HasForeignKey(nf => nf.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Estimate>()
            .HasMany(e => e.ResourceRates)
            .WithOne(rr => rr.Estimate!)
            .HasForeignKey(rr => rr.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Estimate>()
            .HasMany(e => e.RoleMappings)
            .WithOne(m => m.Estimate!)
            .HasForeignKey(m => m.EstimateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoleMapping>()
            .HasIndex(m => new { m.EstimateId, m.SourceRole })
            .IsUnique();

        modelBuilder.Entity<NonFunctionalItem>()
            .HasMany(nf => nf.Allocations)
            .WithOne(a => a.NonFunctionalItem!)
            .HasForeignKey(a => a.NonFunctionalItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
