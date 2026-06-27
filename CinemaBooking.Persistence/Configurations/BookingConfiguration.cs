namespace CinemaBooking.Persistence.Configurations;

using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(b => b.ShowTime)
               .WithMany(s => s.Bookings)
               .HasForeignKey(b => b.ShowTimeId)
               .OnDelete(DeleteBehavior.Restrict);
               
        builder.HasOne<ApplicationUser>()
               .WithMany(u => u.Bookings)
               .HasForeignKey(b => b.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Price).HasColumnType("decimal(18,2)");

        builder.HasOne(i => i.Booking)
               .WithMany(b => b.Items)
               .HasForeignKey(i => i.BookingId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
