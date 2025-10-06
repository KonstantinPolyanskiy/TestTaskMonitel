using System.ComponentModel.DataAnnotations;

namespace Main.WebApi.Models;

/// <summary>
/// Запрос на создание фильма.
/// </summary>
public sealed class CreateMovieRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;

    [Range(1, 3000)] public int Year { get; init; }

    [Range(1, 10000)] public int DurationMinutes { get; init; }

    public string AgeRating { get; init; } = string.Empty;

    [Required, MinLength(1)] public List<int> GenresId { get; init; } = new();

    public IFormFile? Poster { get; init; }
    public bool IsRental { get; init; } = true;
}