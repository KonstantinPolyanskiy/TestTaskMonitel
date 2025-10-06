using System.ComponentModel.DataAnnotations;

namespace Main.WebApi.Models;

/// <summary>
/// Запрос на поиск фильма в показе.
/// </summary>
public sealed class MovieShowtimesRequest
{
    [Required] 
    public Guid MovieId { get; init; }
    public List<Guid>? HallIds { get; init; }
    public DateOnly? On { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To   { get; init; }
}