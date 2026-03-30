using AirlineTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicketing.Infrastructure.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.ToTable("Flights");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FlightNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(f => f.FlightNumber)
            .IsUnique();

        builder.Property(f => f.AirportFrom)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.AirportTo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.DurationMinutes)
            .IsRequired();

        builder.Property(f => f.Capacity)
            .IsRequired();

        builder.Property(f => f.AvailableSeats)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.HasCheckConstraint("CK_Flights_Capacity", "\"Capacity\" > 0");
        builder.HasCheckConstraint("CK_Flights_AvailableSeats", "\"AvailableSeats\" >= 0");
        builder.HasCheckConstraint("CK_Flights_AvailableSeats_Limit", "\"AvailableSeats\" <= \"Capacity\"");
    }
}