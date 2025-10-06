namespace Main.Application.Services.BrowseService.Models;

public sealed record ShowtimeOccurrenceResponse(
    DateOnly Date,
    TimeOnly StartTime,
    Guid HallId,
    string HallName,
    int Price
);