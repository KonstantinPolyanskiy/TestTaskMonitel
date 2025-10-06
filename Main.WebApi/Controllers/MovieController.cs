using AutoMapper;
using Main.WebApi.Controllers.Movie.Models;
using Microsoft.AspNetCore.Mvc;

namespace Main.WebApi.Controllers.Movie;

[ApiController]
[Route("api/v1/movie")]
public sealed class MovieController : ControllerBase
{
    private readonly ILogger<MovieController> _logger;
    private readonly IMapper _mapper;

    public MovieController(ILogger<MovieController> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateMovieRequest req, CancellationToken ct = default)
    {
        var createModel = _mapper.Map<CreateMovieRequest>(req);

    }
}