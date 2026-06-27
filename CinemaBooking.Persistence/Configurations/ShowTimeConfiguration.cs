namespace CinemaBooking.Persistence.Configurations;

using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ShowTimeConfiguration : IEntityTypeConfiguration<ShowTime>
{
    public void Configure(EntityTypeBuilder<ShowTime> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TicketPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(s => s.Movie)
               .WithMany(m => m.ShowTimes)
               .HasForeignKey(s => s.MovieId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Hall)
               .WithMany(h => h.ShowTimes)
               .HasForeignKey(s => s.HallId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
