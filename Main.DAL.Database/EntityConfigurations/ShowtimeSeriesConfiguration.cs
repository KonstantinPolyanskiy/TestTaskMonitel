using Main.Domain.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DAL.Database.EntityConfigurations;

public sealed class ShowtimeSeriesConfiguration : IEntityTypeConfiguration<ShowtimeSeries>
{
    public void Configure(EntityTypeBuilder<ShowtimeSeries> builder)
    {
        builder.ToTable("showtime_series");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.MovieId).IsRequired();
        builder.Property(x => x.HallId).IsRequired();

        builder.Property(x => x.StartTime)
            .IsRequired(); 

        builder.Property(x => x.ActiveFrom)
            .IsRequired();

        builder.Property(x => x.ActiveTo)
            .IsRequired(); 

        builder.Property(x => x.BasePrice)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()          
            .HasMaxLength(16);

        builder.Property(x => x.ActualVersion)
            .IsConcurrencyToken();

        builder.HasIndex(x => x.HallId);
        builder.HasIndex(x => x.MovieId);
        builder.HasIndex(x => new { x.HallId, x.StartTime });
        builder.HasIndex(x => new { x.ActiveFrom, x.ActiveTo });
    }
}