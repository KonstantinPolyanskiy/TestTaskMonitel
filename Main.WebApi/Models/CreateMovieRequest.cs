namespace Main.WebApi.Models;

public sealed class CreateMovieRequest
{
    public string Title { get; init; } = string.Empty;
    public int Year { get; init; } 
    public int DurationMinutes { get; init; } 
    public string AgeRating { get; init; } = string.Empty; 
    public List<int> GenresId { get; init; } = new(); 
    public IFormFile? Poster { get; init; }
    public bool IsRental { get; init; } = true;
}