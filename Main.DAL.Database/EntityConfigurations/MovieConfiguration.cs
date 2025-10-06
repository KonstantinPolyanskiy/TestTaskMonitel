using Main.Domain.Movie;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Main.DAL.Database.EntityConfigurations;

public sealed class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AgeRating)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IsRental)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Duration)
            .IsRequired();

        builder.Property(x => x.PosterKey)
            .HasMaxLength(600);

        builder.Property(x => x.ActualVersion)
            .IsConcurrencyToken();

        builder.HasMany(x => x.Genres)
            .WithOne()
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Genres)
            .HasField("_genres")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Title);
        builder.HasIndex(x => new { x.Title, x.Year });
    }
}