using AirlineTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicketing.Infrastructure.Configurations;

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("CheckIns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SeatNumber)
            .IsRequired();

        builder.Property(c => c.FlightId)
            .IsRequired();

        builder.Property(c => c.CheckInTime)
            .IsRequired();

        builder.HasIndex(c => c.TicketId)
            .IsUnique();

        builder.HasIndex(c => new { c.FlightId, c.SeatNumber })
            .IsUnique();

        builder.HasOne(c => c.Ticket)
            .WithOne(t => t.CheckIn)
            .HasForeignKey<CheckIn>(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Flight)
            .WithMany()
            .HasForeignKey(c => c.FlightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
