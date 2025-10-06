namespace Main.Domain.Movie;

/// <summary>
/// Жанр.
/// </summary>
public sealed class Genre
{
    public int Id { get; private set; }
    
    public string Name { get; private set; } = default!;
}

/// <summary>
/// Жанры фильма.
/// </summary>
public sealed class MovieGenre
{
    public Guid MovieId { get; private set; }
    public int GenreId { get; private set; }

    private MovieGenre() { }

    public static MovieGenre Create(Guid movieId, int genreId)
        => new MovieGenre { MovieId = movieId, GenreId = genreId };
}
