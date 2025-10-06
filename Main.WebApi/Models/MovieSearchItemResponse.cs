namespace Main.Application.Services.BrowseService.Models;

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