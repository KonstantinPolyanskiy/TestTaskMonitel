using Main.Application.Services.BrowseService.Models;

namespace Main.WebApi.Models;

/// <summary>
/// Ответ на запрос показов.
/// </summary>
public sealed record GroupedShowtimesResponse(
    Guid MovieId,
    string Title,
    int Year,
    string AgeRating,
    int Duration,
    int[] GenreIds,
    List<ShowtimeOccurrenceResponse> Showtimes
);