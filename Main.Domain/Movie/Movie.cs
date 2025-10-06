using Main.Domain.Exceptions;

namespace Main.Domain.Movie;

/// <summary>
/// Доменная модель фильма.
/// </summary>
public sealed class Movie : OptimisticLockedEntity
{
    /// <summary>
    /// Id фильма.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Название фильма.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Год выпуска фильма.
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Продолжительность фильма в минутах.
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Возрастной рейтинг для фильма.
    /// </summary>
    public string AgeRating { get; private set; } = string.Empty;
    
    /// <summary>
    /// Доступен ли фильм для проката.
    /// </summary>
    public bool IsRental { get; private set; }

    /// <summary>
    /// Id постера фильма.
    /// </summary>
    public string? PosterKey { get; private set; }

    /// <summary>
    /// Жанры фильма.
    /// </summary>
    public IReadOnlyCollection<MovieGenre> Genres => _genres.AsReadOnly();
    private readonly List<MovieGenre> _genres = new();

    private Movie() { }

    public static Movie Create(
        Guid id,
        string title,
        int year,
        int duration,
        string ageRating,
        bool isRental,
        string? posterKey,
        IEnumerable<int> genreIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(ageRating);

        if (id == Guid.Empty)
        {
            throw new DomainValidationException($"{nameof(id)} не может быть пустым.");
        }

        if (year < 1888 || year > DateTime.UtcNow.Year + 1)
        {
            throw new DomainValidationException($"{nameof(year)} не должен быть меньше 1888 или больше текущего на год.");
        }

        if (duration <= 0)
        {
            throw new DomainValidationException($"{nameof(duration)} должна быть больше нуля минут.");
        }

        var distinctGenres = genreIds.Where(g => g > 0).Distinct().ToArray();
        if (distinctGenres.Length == 0)
        {
            throw new DomainValidationException($"{nameof(genreIds)} должен содержать минимум 1 жанр.");
        }

        var movie = new Movie
        {
            Id = id,
            Title = title,
            Year = year,
            Duration = duration,
            AgeRating = ageRating,
            IsRental = isRental,
            PosterKey = posterKey,
        };

        foreach (var genre in distinctGenres)
        {
            movie._genres.Add(MovieGenre.Create(movie.Id, genre));
        }

        return movie;
    }

    /// <summary>
    /// Снять фильм с проката.
    /// </summary>
    public void RemoveFromRental(int version)
    {
        CheckVersion(version);

        if (!IsRental)
        {
            throw new DomainConflictException("Фильм уже снят с проката");
        }

        IsRental = false;

        UpVersion();
    }
}