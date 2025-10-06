using Main.Domain.Exceptions;

namespace Main.Domain.Hall;

/// <summary>
/// Доменная модель зала.
/// </summary>
public sealed class Hall : OptimisticLockedEntity
{
    /// <summary>
    /// Идентификатор зала.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Название зала.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Количество сидячих мест.
    /// </summary>
    public int Seats { get; private set; }

    /// <summary>
    /// Технический перерыв (в минутах).
    /// </summary>
    public int TechBreak {get; private set; }

    private Hall() { }

    public static Hall Create(Guid id, string name, int seats, int techBreak)
    {
        if (id == Guid.Empty)
        {
            throw new DomainValidationException($"{nameof(id)} не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException($"{nameof(name)} не может быть пустым.");
        }

        if (seats <= 0)
        {
            throw new DomainValidationException($"{nameof(seats)} не может быть меньше или равным 0.");
        }

        return new Hall
        {
            Id = id,
            Name = name,
            Seats = seats,
            TechBreak = techBreak
        };
    }
}