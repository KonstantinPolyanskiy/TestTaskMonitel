using System.ComponentModel.DataAnnotations;

namespace Main.WebApi.Models;

/// <summary>
/// Запрос на создание зала.
/// </summary>
public sealed class CreateHallRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Range(1, 10000)] public int Seats { get; init; }

    [Range(0, 500)] public int TechBreak { get; init; }
}