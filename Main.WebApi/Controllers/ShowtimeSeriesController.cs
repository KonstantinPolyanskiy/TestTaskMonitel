using Main.Application.Services.ShowtimeSeries.Impl;
using Main.Application.Services.ShowtimeSeries.Models;
using Main.Domain.Exceptions;
using Main.Domain.Session;
using Main.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Main.WebApi.Controllers;

[ApiController]
[Route("api/v1/showtime_series")]
public sealed class ShowtimeSeriesController : ControllerBase
{
    private readonly ShowtimeSeriesService _showtimeSeriesService;

    public ShowtimeSeriesController(ShowtimeSeriesService showtimeSeriesService)
    {
        _showtimeSeriesService = showtimeSeriesService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShowtimeSeries), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateShowtimeRequest req, CancellationToken ct = default)
    {
        var model = new CreateShowtimeModel
        {
            MovieId = req.MovieId,
            HallId = req.HallId,
            StartTime = req.StartTime,
            ActiveFrom = req.ActiveFrom,
            ActiveTo = req.ActiveTo,
            BasePrice = req.BasePrice,
            Status = req.Status,
        };

        var showtimeSeries = await _showtimeSeriesService.CreateShowtimeSeriesAsync(model, ct);
        if (showtimeSeries is null)
        {
            return BadRequest();
        }

        return Ok(showtimeSeries);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ShowtimeSeries), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var showtimes = await _showtimeSeriesService.GetAllAsync(ct);

        return Ok(showtimes);
    }
}