using System.ComponentModel.DataAnnotations;

namespace Main.Application.Services.BrowseService.Models;

public sealed class MovieShowtimesRequest
{
    [Required] 
    public Guid MovieId { get; init; }
    public List<Guid>? HallIds { get; init; }
    public DateOnly? On { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To   { get; init; }
}