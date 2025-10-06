namespace Main.Application.Services.BrowseService.Models;

public sealed record MovieSearchItemDto(
    Guid MovieId, string Title, int Year, string AgeRating, int Duration, int[] GenreIds,
    DateOnly? NearestDate, TimeOnly? NearestStartTime, Guid? NearestHallId, string? NearestHallName, int? Price);

public sealed record ShowtimeOccurrenceDto(DateOnly Date, TimeOnly StartTime, Guid HallId, string HallName, int Price);

public sealed record GroupedShowtimesDto(
    Guid MovieId, string Title, int Year, string AgeRating, int Duration, int[] GenreIds,
    IReadOnlyList<ShowtimeOccurrenceDto> Showtimes);