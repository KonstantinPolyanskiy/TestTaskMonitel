using Main.Application.Services.ShowtimeSeries.Models;
using Main.DAL.Database;
using Main.Domain.Cinema;
using Main.Domain.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Main.Application.Services.ShowtimeSeries.Impl;

public sealed class ShowtimeSeriesService
{
    private readonly MainDbContext _db;
    private readonly ILogger _logger;
    private readonly IOptions<CinemaHoursOptions> _hours;

    public ShowtimeSeriesService(MainDbContext db, ILogger<ShowtimeSeriesService> logger, IOptions<CinemaHoursOptions> hours)
    {
        _hours = hours;
        _db = db;
        _logger = logger;
    }

    public async Task<Domain.Session.ShowtimeSeries?> CreateShowtimeSeriesAsync(CreateShowtimeModel req, CancellationToken ct = default)
    {
        var movie = await _db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == req.MovieId, ct);
        if (movie is null)
        {
            return null;
        }

        var hall = await _db.Halls.AsNoTracking().FirstOrDefaultAsync(h => h.Id == req.HallId, ct);
        if (hall is null)
        {
            return null;
        }

        var candidate = Domain.Session.ShowtimeSeries.Create(
            id: Guid.NewGuid(),
            movie: movie,
            hall: hall,
            startTime: req.StartTime,
            activeFrom: req.ActiveFrom,
            activeTo: req.ActiveTo,
            basePrice: req.BasePrice,
            status: req.Status
        );

        var work = new BusinessHours(
            open:  TimeOnly.Parse(_hours.Value.Open),
            close: TimeOnly.Parse(_hours.Value.Close)
        );

        var existing = await _db.ShowtimesSeries
            .Where(s => s.HallId == req.HallId &&
                        s.ActiveFrom <= req.ActiveTo &&
                        s.ActiveTo   >= req.ActiveFrom)
            .AsNoTracking()
            .ToListAsync(ct);

        var otherMovieIds = existing.Select(s => s.MovieId).Distinct().ToArray();
        
        var moviesById = await _db.Movies
            .Where(m => otherMovieIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m, ct);

        ShowtimeScheduler.EnsureCanSchedule(
            candidate,
            candidateMovie: movie,
            candidateHall: hall,
            workHours: work,
            existingSeries: existing,
            moviesById: moviesById,
            validateFrom: req.ActiveFrom,
            validateTo: req.ActiveTo
        );

        _db.ShowtimesSeries.Add(candidate);
        await _db.SaveChangesAsync(ct);

        return candidate;
    }
}