using System.ComponentModel.DataAnnotations;

namespace Main.WebApi.Models;

/// <summary>
/// Запрос для изменения статуса проката фильма.
/// </summary>
public sealed class ChangeRentalRequest
{
    [Required] public int Version { get; init; }
}