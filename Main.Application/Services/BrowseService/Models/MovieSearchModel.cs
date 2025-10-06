namespace Main.Application.Services.BrowseService.Models;

public sealed class MovieSearchModel
{
    public string? Q { get; init; }
    public int[] GenreIds { get; init; } = [];
    public int? YearFrom { get; init; }
    public int? YearTo   { get; init; }
    public string? AgeRating { get; init; }
    public int? DurationMin { get; init; }
    public int? DurationMax { get; init; }
    public int? PriceMin { get; init; }
    public int? PriceMax { get; init; }
    public DateOnly On { get; init; }
}
