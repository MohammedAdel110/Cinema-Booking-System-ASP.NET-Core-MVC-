namespace CinemaBooking.Persistence.Configurations;

using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(2000);

        builder.HasOne(m => m.Category)
               .WithMany(c => c.Movies)
               .HasForeignKey(m => m.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Poster)
               .WithOne(p => p.Movie)
               .HasForeignKey<Poster>(p => p.MovieId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
