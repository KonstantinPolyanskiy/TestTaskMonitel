using AutoMapper;
using Main.Application.Services.MovieService.Impl;
using Main.Application.Services.MovieService.Models;
using Main.Domain.Movie;
using Main.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Main.WebApi.Controllers;

[ApiController]
[Route("api/v1/movie")]
public sealed class MovieController : ControllerBase
{
    private readonly ILogger<MovieController> _logger;
    private readonly MovieService _movieService;

    public MovieController(ILogger<MovieController> logger, MovieService movieService)
    {
        _logger = logger;
        _movieService = movieService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateMovieRequest req, CancellationToken ct = default)
    {
        var model = new CreateMovieModel
        {
            Title = req.Title,
            Year = req.Year,
            DurationMinutes = req.DurationMinutes,
            AgeRating = req.AgeRating,
            GenresId = req.GenresId,
            IsRental = req.IsRental,
        };

        Stream? posterStream = null;
        if (req.Poster is { Length: > 0 })
        {
            posterStream = req.Poster.OpenReadStream();
        }

        var movie = await _movieService.CreateMovieAsync(model, posterStream, ct);

        return Ok(movie);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var movies = await _movieService.GetAllAsync(ct);

        return Ok(movies);
    }

    [HttpPost("{id:guid}/remove-from-rental")]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveFromRental([FromRoute] Guid id, [FromBody] ChangeRentalRequest body, CancellationToken ct)
    {
        var movie = await _movieService.RemoveFromRentalAsync(id, body.Version, ct);
        return Ok(movie);
    }
}