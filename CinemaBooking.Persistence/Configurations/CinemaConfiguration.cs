namespace CinemaBooking.Persistence.Configurations;

using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CinemaConfiguration : IEntityTypeConfiguration<Cinema>
{
    public void Configure(EntityTypeBuilder<Cinema> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Location).IsRequired().HasMaxLength(500);
    }
}

public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(h => h.Cinema)
               .WithMany(c => c.Halls)
               .HasForeignKey(h => h.CinemaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
