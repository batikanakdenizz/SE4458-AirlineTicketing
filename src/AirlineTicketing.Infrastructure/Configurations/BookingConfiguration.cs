using AirlineTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicketing.Infrastructure.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.PnrCode)
            .IsRequired()
            .HasMaxLength(8);

        builder.HasIndex(b => b.PnrCode)
            .IsUnique();

        builder.Property(b => b.IdempotencyKey)
            .HasMaxLength(100);

        builder.HasIndex(b => b.IdempotencyKey)
            .IsUnique();

        builder.Property(b => b.ContactEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.ContactPhone)
            .HasMaxLength(50);

        builder.Property(b => b.Status)
            .IsRequired();

        builder.Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.HasOne(b => b.Flight)
            .WithMany(f => f.Bookings)
            .HasForeignKey(b => b.FlightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
