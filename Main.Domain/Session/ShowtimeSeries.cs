using Main.Domain.Exceptions;

namespace Main.Domain.Session;

/// <summary>
/// Статус показа.
/// </summary>
public enum ShowtimeStatus
{
    Active,
    Cancelled,
}

public sealed class ShowtimeSeries : OptimisticLockedEntity
{
    /// <summary>
    /// Id серии сеанса.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Id фильма, который показывают в сеансе.
    /// </summary>
    public Guid MovieId { get; private set; }

    /// <summary>
    /// Id зала, в котором проходит сеанс.
    /// </summary>
    public Guid HallId { get; private set; }

    /// <summary>
    /// Ежедневное время начала.
    /// </summary>
    public TimeOnly StartTime { get; private set; }

    /// <summary>
    /// Дата, с которой проходит показ.
    /// </summary>
    public DateOnly ActiveFrom { get; private set; }

    /// <summary>
    /// Дата, по которую происходит показ.
    /// </summary>
    public DateOnly ActiveTo { get; private set; }

    /// <summary>
    /// Базовая цена (в рублях), до применения скидки.
    /// </summary>
    public int BasePrice { get; private set; }

    /// <summary>
    /// Статус показа.
    /// </summary>
    public ShowtimeStatus Status { get; private set; }

    private ShowtimeSeries() { }

    public static ShowtimeSeries Create(
        Guid id,
        Movie.Movie movie,
        Hall.Hall hall,
        TimeOnly startTime,
        DateOnly activeFrom,
        DateOnly activeTo,
        int basePrice,
        ShowtimeStatus status)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException($"{nameof(id)} не может быть пустым.");
        }

        if (!movie.IsRental)
        {
            throw new DomainValidationException($"{nameof(movie)} не может быть добавлен будучи снятым с проката.");
        }

        if (activeTo < activeFrom)
        {
            throw new DomainValidationException($"{nameof(activeTo)} раньше, чем {nameof(activeFrom)}.");
        }

        if (basePrice < 0)
        {
            throw new DomainValidationException($"{nameof(basePrice)} не может быть меньше 0.");
        }

        return new ShowtimeSeries
        {
            Id = id,
            MovieId = movie.Id,
            HallId  = hall.Id,
            StartTime = startTime,
            ActiveFrom = activeFrom,
            ActiveTo   = activeTo,
            BasePrice  = basePrice,
            Status = status
        };
    }

    /// <summary>
    /// Отменить показ.
    /// </summary>
    public void Cancel()
    {
        if (Status == ShowtimeStatus.Cancelled)
        {
            return;
        }

        Status = ShowtimeStatus.Cancelled;

        UpVersion();
    }

    /// <summary>
    /// Возвращает true, в случае если переданная дата попадает в активный интервал серии.
    /// </summary>
    public bool IsActiveOn(DateOnly date) => date >= ActiveFrom && date <= ActiveTo;
}