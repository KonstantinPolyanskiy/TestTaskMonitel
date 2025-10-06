using Main.Application.Contracts;
using Main.Application.Services.MovieService.Models;
using Main.DAL.Database;
using Main.Domain.Exceptions;
using Main.Domain.Movie;
using Main.Domain.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Services.MovieService.Impl;

public sealed class MovieService
{
    private readonly ILogger<MovieService> _logger;
    private readonly IFileStorage _fileStorage;
    private readonly MainDbContext _db;

    public MovieService(ILogger<MovieService> logger, IFileStorage fileStorage, MainDbContext db)
    {
        _logger = logger;
        _fileStorage = fileStorage;
        _db = db;
    }

    public async Task<Movie> CreateMovieAsync(CreateMovieModel req, Stream? postStream = null,
        CancellationToken ct = default)
    {
        var distinctGenreIds = req.GenresId
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (distinctGenreIds.Length == 0)
        {
            throw new DomainNotFoundException("Нужно указать минимум 1 жанр.");
        }

        var existingIds = await _db.Genres
            .Where(g => distinctGenreIds.Contains(g.Id))
            .Select(g => g.Id)
            .ToArrayAsync(ct);

        var missing = distinctGenreIds.Except(existingIds).ToArray();
        if (missing.Length > 0)
        {
            throw new DomainNotFoundException($"Часть переданных жанров не существует: [{string.Join(", ", missing)}].");
        }

        var movieId  = Guid.NewGuid();
        string? posterKey = null;

        if (postStream is not null)
        {
            posterKey = await _fileStorage.UploadFileAsync(postStream, movieId.ToString());
        }

        var movie = Movie.Create(
            id: movieId,
            title: req.Title.Trim(),
            year: req.Year,
            duration: req.DurationMinutes,
            ageRating: req.AgeRating.Trim(),
            isRental: req.IsRental,
            posterKey: posterKey,
            genreIds: distinctGenreIds
        );

        _db.Movies.Add(movie);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Создан фильм {MovieId} '{Title}' ({Year})", movie.Id, movie.Title, movie.Year);
        return movie;
    }

    public async Task<Movie> RemoveFromRentalAsync(Guid movieId, int version, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var movie = await _db.Movies.FirstOrDefaultAsync(m => m.Id == movieId, ct);
        if (movie is null)
        {
            throw new DomainNotFoundException($"Фильм {movieId} не найден.");
        }

        movie.RemoveFromRental(version);

        var series = await _db.ShowtimesSeries
            .Where(s => s.MovieId == movieId && s.Status == ShowtimeStatus.Active)
            .ToListAsync(ct);

        foreach (var s in series)
        {
            s.Cancel();
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        _logger.LogInformation("Фильм {MovieId} снят с проката, отменено серий: {Count}", movieId, series.Count);
        
        return movie;
    }

    public async Task<List<Movie>> GetAllAsync(CancellationToken ct = default)
    {
        var movies = await _db.Movies.AsNoTracking().Include(m => m.Genres).ToListAsync(ct);

        return movies;
    }
}