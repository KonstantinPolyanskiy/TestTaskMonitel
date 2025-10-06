namespace Main.Application.Services.BrowseService.Models;

public sealed record GroupedShowtimesResponse(
    Guid MovieId,
    string Title,
    int Year,
    string AgeRating,
    int Duration,
    int[] GenreIds,
    List<ShowtimeOccurrenceResponse> Showtimes
);