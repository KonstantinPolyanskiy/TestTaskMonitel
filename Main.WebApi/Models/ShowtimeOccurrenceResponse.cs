namespace Main.WebApi.Models;

/// <summary>
/// Ответ на запрос поиска фильма в показе.
/// </summary>
public sealed record ShowtimeOccurrenceResponse(
    DateOnly Date,
    TimeOnly StartTime,
    Guid HallId,
    string HallName,
    int Price
);