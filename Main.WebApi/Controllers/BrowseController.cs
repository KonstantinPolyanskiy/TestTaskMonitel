using Main.Application.Services.BrowseService.Impl;
using Main.Application.Services.BrowseService.Models;
using Main.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Main.WebApi.Controllers;

[ApiController]
[Route("api/v1/browse")]
public sealed class BrowseController : ControllerBase
{
    private readonly BrowseService _browseService;
    private readonly ILogger<BrowseController> _logger;

    public BrowseController(BrowseService browseService, ILogger<BrowseController> logger)
    {
        _browseService = browseService;
        _logger = logger;
    }

    [HttpGet("movies")]
    public async Task<IActionResult> SearchMovies([FromQuery] MovieSearchRequest req, CancellationToken ct)
    {
        _logger.LogDebug("Запрос на поиск фильмов: {req}", req);

        var model = new MovieSearchModel
        {
            Q = req.Q,
            GenreIds = (req.GenreIds ?? []).Distinct().ToArray(),
            YearFrom = req.YearFrom,
            YearTo   = req.YearTo,
            AgeRating = req.AgeRating,
            DurationMin = req.DurationMin,
            DurationMax = req.DurationMax,
            PriceMin = req.PriceMin,
            PriceMax = req.PriceMax,
            On = req.On ?? DateOnly.FromDateTime(DateTime.UtcNow.Date)
        };

        var items = await _browseService.SearchMoviesAsync(model, ct);

        var resp = items.Select(x => new MovieSearchItemResponse(
            x.MovieId, x.Title, x.Year, x.AgeRating, x.Duration, x.GenreIds,
            x.NearestDate, x.NearestStartTime, x.NearestHallId, x.NearestHallName, x.Price));

        return Ok(resp);
    }

    [HttpGet("showtimes")]
    public async Task<IActionResult> GetActiveShowtimes([FromQuery] ActiveShowtimesRequest req, CancellationToken ct)
    {
        _logger.LogDebug("Запрос на поиск показов: {req}", req);

        DateOnly from, to;
        if (req.On is { } on)
        {
            from = to = on;
        }
        else
        {
            from = req.From ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            to   = req.To ?? from;
            if (to < from)
            {
                return BadRequest(new { message = "To раньше, чем From." });
            }
        }

        var model = new ActiveShowtimesModel
        {
            GenreIds = (req.GenreIds ?? []).Distinct().ToArray(),
            HallIds  = (req.HallIds  ?? []).Distinct().ToArray(),
            YearFrom = req.YearFrom,
            YearTo   = req.YearTo,
            AgeRating = req.AgeRating,
            DurationMin = req.DurationMin,
            DurationMax = req.DurationMax,
            PriceMin = req.PriceMin,
            PriceMax = req.PriceMax,
            From = from,
            To   = to
        };

        var items = await _browseService.GetActiveShowtimesAsync(model, ct);

        var resp = items.Select(g => new GroupedShowtimesResponse(
            g.MovieId, g.Title, g.Year, g.AgeRating, g.Duration, g.GenreIds,
            g.Showtimes.Select(o => new ShowtimeOccurrenceResponse(o.Date, o.StartTime, o.HallId, o.HallName, o.Price)).ToList()
        ));

        return Ok(resp);
    }

    [HttpGet("movies/{movieId:guid}/showtimes")]
    public async Task<IActionResult> GetMovieShowtimes([FromRoute] Guid movieId, [FromQuery] MovieShowtimesRequest req, CancellationToken ct)
    {
        _logger.LogDebug("Запрос на поиск сеансов выбранного фильма: {req}", req);

        if (movieId == Guid.Empty)
        {
            return BadRequest(new { message = "Некорректный MovieId." });
        }

        DateOnly from, to;

        if (req.On is { } on)
        {
            from = to = on;
        }
        else
        {
            from = req.From ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
            to   = req.To ?? from;
            if (to < from)
            {
                return BadRequest(new { message = "To раньше, чем From." });
            }
        }

        var model = new MovieShowtimesModel
        {
            MovieId = movieId,
            HallIds = (req.HallIds ?? new()).Distinct().ToArray(),
            From = from,
            To   = to
        };

        var res = await _browseService.GetMovieShowtimesAsync(model, ct);
        if (res is null)
        {
            return NotFound();
        }

        var resp = new GroupedShowtimesResponse(
            res.MovieId, res.Title, res.Year, res.AgeRating, res.Duration, res.GenreIds,
            res.Showtimes.Select(o => new ShowtimeOccurrenceResponse(o.Date, o.StartTime, o.HallId, o.HallName, o.Price)).ToList()
        );

        return Ok(resp);
    }
}