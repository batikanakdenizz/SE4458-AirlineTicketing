using AirlineTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirlineTicketing.Infrastructure.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TicketNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.TicketNumber)
            .IsUnique();

        builder.Property(t => t.PassengerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.PurchaseDate)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired();

        builder.HasOne(t => t.Flight)
            .WithMany(f => f.Tickets)
            .HasForeignKey(t => t.FlightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}