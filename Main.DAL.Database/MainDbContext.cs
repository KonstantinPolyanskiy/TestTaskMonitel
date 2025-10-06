using Main.Domain.Hall;
using Main.Domain.Movie;
using Main.Domain.Session;
using Microsoft.EntityFrameworkCore;

namespace Main.DAL.Database;

public class MainDbContext : DbContext
{
    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<ShowtimeSeries> ShowtimesSeries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
    }
}