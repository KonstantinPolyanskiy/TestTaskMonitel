namespace Main.Application.Services.BrowseService.Models;

public sealed class MovieShowtimesModel
{
    public Guid MovieId { get; init; }
    public Guid[] HallIds { get; init; } = [];
    public DateOnly From { get; init; }
    public DateOnly To   { get; init; }
}