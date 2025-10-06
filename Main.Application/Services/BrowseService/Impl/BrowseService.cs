using Main.Application.Services.BrowseService.Models;
using Main.DAL.Database;
using Main.Domain.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Services.BrowseService.Impl;

public sealed class BrowseService
{
    private readonly MainDbContext _db;
    private readonly ILogger<BrowseService> _logger;

    public BrowseService(MainDbContext db, ILogger<BrowseService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MovieSearchItemDto>> SearchMoviesAsync(MovieSearchModel m, CancellationToken ct = default)
    {
        var on = m.On;

        var movies = _db.Movies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(m.Q))
        {
            var like = $"%{m.Q.Trim()}%";
            movies = movies.Where(x => EF.Functions.ILike(x.Title, like));
        }

        if (m.GenreIds.Length > 0)
        {
            movies = movies.Where(x => x.Genres.Any(g => m.GenreIds.Contains(g.GenreId)));
        }

        if (m.YearFrom is { } yf)
        {
            movies = movies.Where(x => x.Year >= yf);
        }

        if (m.YearTo is { } yt)
        {
            movies = movies.Where(x => x.Year <= yt);
        }

        if (!string.IsNullOrWhiteSpace(m.AgeRating))
        {
            movies = movies.Where(x => x.AgeRating == m.AgeRating);
        }

        if (m.DurationMin is { } dmin)
        {
            movies = movies.Where(x => x.Duration >= dmin);
        }

        if (m.DurationMax is { } dmax)
        {
            movies = movies.Where(x => x.Duration <= dmax);
        }

        var list = await movies
            .Select(x => new
            {
                x.Id, x.Title, x.Year, x.AgeRating, x.Duration,
                GenreIds = x.Genres.Select(g => g.GenreId).ToArray(),

                Nearest = _db.ShowtimesSeries
                    .Where(s => s.Status == ShowtimeStatus.Active && s.MovieId == x.Id && s.ActiveTo >= on)
                    .Select(s => new
                    {
                        FirstDate = on < s.ActiveFrom ? s.ActiveFrom : on,
                        s.ActiveTo,
                        s.StartTime,
                        s.HallId,
                        s.BasePrice
                    })
                    .Where(y => y.FirstDate <= y.ActiveTo)
                    .OrderBy(y => y.FirstDate).ThenBy(y => y.StartTime)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var hallIds = list.Where(r => r.Nearest != null)
            .Select(r => r.Nearest!.HallId)
            .Distinct()
            .ToArray();

        var hallsById = await _db.Halls.AsNoTracking()
            .Where(h => hallIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id, h => h.Name, ct);

        var mapped = list.Select(r =>
        {
            var price = r.Nearest?.BasePrice;

            return new MovieSearchItemDto(
                r.Id, r.Title, r.Year, r.AgeRating, r.Duration, r.GenreIds,
                r.Nearest?.FirstDate, r.Nearest?.StartTime, r.Nearest?.HallId,
                r.Nearest?.HallId is { } hid ? hallsById.GetValueOrDefault(hid) : null,
                price
            );
        });

        if (m.PriceMin is { } pmin)
        {
            mapped = mapped.Where(x => x.Price is { } p && p >= pmin);
        }

        if (m.PriceMax is { } pmax)
        {
            mapped = mapped.Where(x => x.Price is { } p && p <= pmax);
        }

        return mapped.ToList();
    }

    public async Task<IReadOnlyList<GroupedShowtimesDto>> GetActiveShowtimesAsync(
        ActiveShowtimesModel m, CancellationToken ct = default)
    {
        var baseQuery = _db.ShowtimesSeries
            .AsNoTracking()
            .Where(s => s.Status == ShowtimeStatus.Active
                        && s.ActiveFrom <= m.To
                        && s.ActiveTo >= m.From)
            .Join(
                _db.Movies.AsNoTracking(),
                s => s.MovieId,
                mv => mv.Id,
                (s, mv) => new { s, mv }
            );

        if (m.HallIds.Length > 0)
        {
            baseQuery = baseQuery.Where(x => m.HallIds.Contains(x.s.HallId));
        }

        if (m.GenreIds.Length > 0)
        {
            baseQuery = baseQuery.Where(x => x.mv.Genres.Any(g => m.GenreIds.Contains(g.GenreId)));
        }

        if (m.YearFrom is { } yf)
        {
            baseQuery = baseQuery.Where(x => x.mv.Year >= yf);
        }

        if (m.YearTo is { } yt)
        {
            baseQuery = baseQuery.Where(x => x.mv.Year <= yt);
        }

        if (!string.IsNullOrWhiteSpace(m.AgeRating))
        {
            baseQuery = baseQuery.Where(x => x.mv.AgeRating == m.AgeRating);
        }

        if (m.DurationMin is { } dmin)
        {
            baseQuery = baseQuery.Where(x => x.mv.Duration >= dmin);
        }

        if (m.DurationMax is { } dmax)
        {
            baseQuery = baseQuery.Where(x => x.mv.Duration <= dmax);
        }

        var rows = await baseQuery
            .Select(x => new
            {
                Series = new
                {
                    x.s.Id, x.s.MovieId, x.s.HallId, x.s.StartTime, x.s.ActiveFrom, x.s.ActiveTo, x.s.BasePrice
                },
                Movie = new
                {
                    x.mv.Id, x.mv.Title, x.mv.Year, x.mv.AgeRating, x.mv.Duration,
                    GenreIds = x.mv.Genres.Select(g => g.GenreId).ToArray()
                }
            })
            .ToListAsync(ct);

        var hallIds = rows.Select(r => r.Series.HallId)
            .Distinct()
            .ToArray();
        
        var hallsById = await _db.Halls.AsNoTracking()
            .Where(h => hallIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id, h => h.Name, ct);

        var perMovie = new Dictionary<Guid, List<ShowtimeOccurrenceDto>>();

        foreach (var r in rows)
        {
            var start = Max(r.Series.ActiveFrom, m.From);
            var stop = Min(r.Series.ActiveTo, m.To);
            if (stop < start) continue;

            for (var d = start; d <= stop; d = d.AddDays(1))
            {
                var price = r.Series.BasePrice;

                if (m.PriceMin is { } pmin && price < pmin) continue;
                if (m.PriceMax is { } pmax && price > pmax) continue;

                var occ = new ShowtimeOccurrenceDto(
                    d, r.Series.StartTime, r.Series.HallId,
                    hallsById.GetValueOrDefault(r.Series.HallId) ?? "", price);

                if (!perMovie.TryGetValue(r.Movie.Id, out var list))
                {
                    list = new List<ShowtimeOccurrenceDto>();
                    perMovie[r.Movie.Id] = list;
                }

                list.Add(occ);
            }
        }

        var moviesMeta = rows.Select(r => r.Movie).DistinctBy(mv => mv.Id).ToDictionary(mv => mv.Id);

        var result = perMovie.Select(kvp =>
            {
                var meta = moviesMeta[kvp.Key];
                var sorted = kvp.Value.OrderBy(o => o.Date).ThenBy(o => o.StartTime).ToList();

                return new GroupedShowtimesDto(
                    kvp.Key, meta.Title, meta.Year, meta.AgeRating, meta.Duration, meta.GenreIds, sorted);
            })
            .OrderBy(g => g.Showtimes.First().Date)
            .ThenBy(g => g.Showtimes.First().StartTime)
            .ToList();

        return result;
    }

    public async Task<GroupedShowtimesDto?> GetMovieShowtimesAsync(MovieShowtimesModel m, CancellationToken ct = default)
    {
        var movie = await _db.Movies.AsNoTracking()
            .Include(x => x.Genres)
            .FirstOrDefaultAsync(x => x.Id == m.MovieId, ct);

        if (movie is null) return null;

        var series = await _db.ShowtimesSeries.AsNoTracking()
            .Where(s => s.Status == ShowtimeStatus.Active
                        && s.MovieId == m.MovieId
                        && s.ActiveFrom <= m.To && s.ActiveTo >= m.From)
            .ToListAsync(ct);

        if (m.HallIds.Length > 0)
        {
            series = series.Where(s => m.HallIds.Contains(s.HallId)).ToList();
        }

        var hallIds = series.Select(s => s.HallId)
            .Distinct()
            .ToArray();
        
        var hallsById = await _db.Halls.AsNoTracking()
            .Where(h => hallIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id, h => h.Name, ct);

        var occ = new List<ShowtimeOccurrenceDto>();
        foreach (var s in series)
        {
            var start = Max(s.ActiveFrom, m.From);
            var stop  = Min(s.ActiveTo,   m.To);
            
            for (var d = start; d <= stop; d = d.AddDays(1))
            {
                occ.Add(new ShowtimeOccurrenceDto(
                    d, s.StartTime, s.HallId, hallsById.GetValueOrDefault(s.HallId) ?? "", s.BasePrice));
            }
        }

        var sorted = occ.OrderBy(o => o.Date).ThenBy(o => o.StartTime).ToList();

        return new GroupedShowtimesDto(
            movie.Id, movie.Title, movie.Year, movie.AgeRating, movie.Duration,
            movie.Genres.Select(g => g.GenreId).ToArray(), sorted);
    }

    private static DateOnly Max(DateOnly a, DateOnly b) => a >= b ? a : b;
    private static DateOnly Min(DateOnly a, DateOnly b) => a <= b ? a : b;
    
}