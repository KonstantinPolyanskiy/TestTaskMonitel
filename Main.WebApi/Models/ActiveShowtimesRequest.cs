namespace Main.Application.Services.BrowseService.Models;

public sealed class ActiveShowtimesRequest
{
    public List<int>? GenreIds { get; init; }
    public List<Guid>? HallIds { get; init; }
    public int? YearFrom { get; init; }
    public int? YearTo   { get; init; }
    public string? AgeRating { get; init; }
    public int? DurationMin { get; init; }
    public int? DurationMax { get; init; }
    public int? PriceMin { get; init; }
    public int? PriceMax { get; init; }
    public DateOnly? On { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To   { get; init; }
}