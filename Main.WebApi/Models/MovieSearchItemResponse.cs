namespace Main.WebApi.Models;

/// <summary>
/// Ответ на запрос поиска фильма. 
/// </summary>
public sealed record MovieSearchItemResponse(
    Guid MovieId,
    string Title,
    int Year,
    string AgeRating,
    int Duration,
    int[] GenreIds,
    DateOnly? NearestDate,
    TimeOnly? NearestStartTime,
    Guid? NearestHallId,
    string? NearestHallName,
    int? Price
);