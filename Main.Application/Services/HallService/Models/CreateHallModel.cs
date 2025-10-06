namespace Main.Application.Services.HallService.Models;

public class CreateHallModel
{
    public string Name { get; init; } = string.Empty;
    public int Seats { get; init; }
    public int TechBreak { get; init; }
}