namespace Main.Application.Services.MovieService.Models;

public sealed class CreateMovieModel
{
    public string Title { get; init; } = string.Empty;
    public int Year { get; init; }
    public int DurationMinutes { get; init; }
    public string AgeRating { get; init; } = string.Empty;
    public List<int> GenresId { get; init; } = new();
    public bool IsRental { get; init; } = true;
}