using Main.Application.Services.HallService.Impl;
using Main.Application.Services.HallService.Models;
using Main.Domain.Hall;
using Main.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Main.WebApi.Controllers;

[ApiController]
[Route("api/v1/hall")]
public sealed class HallController : ControllerBase 
{
    private readonly HallService _hallService;

    public HallController(HallService hallService)
    {
        _hallService = hallService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Hall), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateHallRequest req, CancellationToken ct = default)
    {
        var model = new CreateHallModel
        {
            Name = req.Name,
            Seats = req.Seats,
            TechBreak = req.TechBreak,
        };

        var hall = await _hallService.CreateAsync(model, ct);

        return Ok(hall);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var halls = await _hallService.GetAllAsync(ct);

        return Ok(halls);
    }
}