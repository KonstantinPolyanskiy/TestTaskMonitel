using Main.Domain.Session;

namespace Main.Application.Services.ShowtimeSeries.Models;

public sealed record ShowtimeSeriesDto(
    Guid Id,
    Guid MovieId,
    string MovieTitle,
    Guid HallId,
    string HallName,
    TimeOnly StartTime,
    DateOnly ActiveFrom,
    DateOnly ActiveTo,
    int BasePrice,
    ShowtimeStatus Status
);