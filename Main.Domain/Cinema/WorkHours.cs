namespace Main.Domain.Cinema;

public readonly struct BusinessHours
{
    public TimeOnly Open { get; }
    public TimeOnly Close { get; }

    public BusinessHours(TimeOnly open, TimeOnly close)
    {
        if (close <= open)
        {
            throw new ArgumentException("Время закрытия должно быть позже открытия.");
        }

        Open = open;
        Close = close;
    }

    /// <summary>
    /// Возвращает true в случае, если
    /// окно времени открытия и закрытия полностью в пределах рабочих часов.
    /// </summary>
    public bool Fits(TimeOnly start, TimeOnly end)
    {
        if (start < Open || start >= Close) return false;

        if (end <= start) return false;

        if (end > Close) return false;

        return true;
    }
}