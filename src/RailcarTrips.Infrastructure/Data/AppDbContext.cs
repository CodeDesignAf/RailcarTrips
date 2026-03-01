using Microsoft.EntityFrameworkCore;
using RailcarTrips.Infrastructure.Data.Entities;

namespace RailcarTrips.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<CityEntity> Cities => Set<CityEntity>();
    public DbSet<EquipmentEventEntity> EquipmentEvents => Set<EquipmentEventEntity>();
    public DbSet<TripEntity> Trips => Set<TripEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CityEntity>(entity =>
        {
            entity.ToTable("Cities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(200);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<EquipmentEventEntity>(entity =>
        {
            entity.ToTable("EquipmentEvents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EquipmentId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Code).IsRequired().HasMaxLength(20);
            entity.Property(x => x.NaturalKeyHash).IsRequired().HasMaxLength(128);
            // DateTime values are stored as UTC timestamps exactly as provided.
            entity.Property(x => x.EventUtcTime).IsRequired();
            entity.Property(x => x.EventLocalTime);

            entity.HasIndex(x => x.NaturalKeyHash).IsUnique();
            entity.HasIndex(x => new { x.EquipmentId, x.EventUtcTime });
        });

        modelBuilder.Entity<TripEntity>(entity =>
        {
            entity.ToTable("Trips");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EquipmentId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Status).IsRequired().HasMaxLength(20);
            entity.Property(x => x.InvalidReason).HasMaxLength(500);
            entity.Property(x => x.StartUtc).IsRequired();

            entity.HasIndex(x => new { x.EquipmentId, x.StartUtc });
        });
    }
}
