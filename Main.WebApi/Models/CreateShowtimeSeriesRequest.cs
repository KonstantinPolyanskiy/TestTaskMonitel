using System.ComponentModel.DataAnnotations;
using Main.Domain.Session;

namespace Main.WebApi.Models;

/// <summary>
/// Запрос на создание сеанса.
/// </summary>
public sealed class CreateShowtimeRequest
{
    [Required] public Guid MovieId { get; init; }

    [Required] public Guid HallId { get; init; }

    [Required, DataType(DataType.Time)] public TimeOnly StartTime { get; init; }

    [Required, DataType(DataType.Date)] public DateOnly ActiveFrom { get; init; }

    [Required, DataType(DataType.Date)] public DateOnly ActiveTo { get; init; }

    [Range(0, int.MaxValue)] public int BasePrice { get; init; }

    [Required, EnumDataType(typeof(ShowtimeStatus))]
    public ShowtimeStatus Status { get; init; } = ShowtimeStatus.Active;
}