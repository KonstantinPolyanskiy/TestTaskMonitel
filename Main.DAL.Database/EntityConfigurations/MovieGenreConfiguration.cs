using Main.Domain.Movie;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DAL.Database.EntityConfigurations;

public sealed class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
{
    public void Configure(EntityTypeBuilder<MovieGenre> b)
    {
        b.HasKey(x => new { x.MovieId, x.GenreId });

        b.HasOne<Movie>()
            .WithMany(m => m.Genres)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<Genre>()
            .WithMany()
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.MovieId);
        b.HasIndex(x => x.GenreId);
    }
}