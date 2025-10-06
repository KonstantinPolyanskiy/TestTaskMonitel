using Main.Domain.Session;

namespace Main.Application.Services.ShowtimeSeries.Models;

public sealed class CreateShowtimeModel
{
    public Guid MovieId { get; init; }
    public Guid HallId  { get; init; }
    public TimeOnly StartTime { get; init; }
    public DateOnly ActiveFrom { get; init; }
    public DateOnly ActiveTo   { get; init; }
    public int BasePrice { get; init; }
    public ShowtimeStatus Status { get; init; } = ShowtimeStatus.Active;
}