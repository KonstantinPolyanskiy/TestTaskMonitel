using Main.Domain.Hall;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DAL.Database.EntityConfigurations;

public sealed class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Seats)
            .IsRequired();

        builder.Property(x => x.TechBreak)
            .IsRequired();

        builder.Property(x => x.ActualVersion)
            .IsConcurrencyToken();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}