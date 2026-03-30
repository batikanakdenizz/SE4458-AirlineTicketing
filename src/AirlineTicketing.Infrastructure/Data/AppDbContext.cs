using AirlineTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineTicketing.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}