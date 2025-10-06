using Main.Domain.Cinema;
using Main.Domain.Exceptions;

namespace Main.Domain.Session;

public static class ShowtimeScheduler
{
    /// <summary>
    /// Проверка, что кандидат может быть запланирован.
    /// Бросает исключение при нарушении инвариантов (часы работы, пересечение, фильм снят и т.п.).
    /// </summary>
    public static void EnsureCanSchedule(
        ShowtimeSeries candidate,
        Movie.Movie candidateMovie,
        Hall.Hall candidateHall,
        BusinessHours workHours,
        IReadOnlyList<ShowtimeSeries> existingSeries,
        IReadOnlyDictionary<Guid, Movie.Movie> moviesById,
        DateOnly validateFrom,
        DateOnly validateTo)
    {
        if (validateTo < validateFrom)
        {
            throw new DomainConflictException($"{nameof(validateTo)} раньше, чем {nameof(validateFrom)}");
        }

        if (candidate.Status != ShowtimeStatus.Active)
        {
            return;
        }

        if (!candidateMovie.IsRental)
        {
            throw new DomainConflictException("Фильм кандидата снят с проката.");
        }

        var endWithBreak = candidate.StartTime
            .AddMinutes(candidateMovie.Duration + candidateHall.TechBreak);

        if (!workHours.Fits(candidate.StartTime, endWithBreak))
        {
            throw new DomainConflictException("Сеанс не укладывается в часы работы.");
        }

        var from = MaxDate(candidate.ActiveFrom, validateFrom);
        var to = MinDate(candidate.ActiveTo, validateTo);
        if (to < from)
        {
            return;
        }

        foreach (var day in Enumerate(from, to))
        {
            var (aStart, aEndBusy) =
                BusyWindow(day, candidate.StartTime, candidateMovie.Duration, candidateHall.TechBreak);

            foreach (var other in existingSeries)
            {
                if (other.Id == candidate.Id) continue;
                if (other.Status != ShowtimeStatus.Active) continue;
                if (other.HallId != candidateHall.Id) continue;
                if (!other.IsActiveOn(day)) continue;

                if (!moviesById.TryGetValue(other.MovieId, out var otherMovie))
                {
                    throw new DomainNotFoundException($"Отсутствует фильм {other.MovieId} в moviesById.");
                }

                if (otherMovie.Duration <= 0)
                    throw new DomainValidationException($"Длительность фильма {other.MovieId} должна быть > 0.");

                var (bStart, bEndBusy) = BusyWindow(
                    day, other.StartTime, otherMovie.Duration, candidateHall.TechBreak);

                if (Overlaps(aStart, aEndBusy, bStart, bEndBusy))
                {
                    throw new DomainConflictException("Конфликт расписания в зале.");
                }
            }
        }
    }

    private static (DateTime Start, DateTime EndBusy) BusyWindow(
        DateOnly date, TimeOnly start, int movieMinutes, int techBreakMinutes)
    {
        var s = date.ToDateTime(start, DateTimeKind.Unspecified);
        var e = s.AddMinutes(movieMinutes + techBreakMinutes);
        return (s, e);
    }

    private static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        => aStart < bEnd && bStart < aEnd;

    private static IEnumerable<DateOnly> Enumerate(DateOnly from, DateOnly to)
    {
        for (var d = from; d <= to; d = d.AddDays(1))
            yield return d;
    }

    private static DateOnly MaxDate(DateOnly a, DateOnly b) => a >= b ? a : b;
    private static DateOnly MinDate(DateOnly a, DateOnly b) => a <= b ? a : b;
}