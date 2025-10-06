using Main.Domain.Hall;
using Main.Domain.Movie;
using Main.Domain.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.DAL.Database.Seeds;

public static class DevDataSeeder
{
    private static readonly Random Rng = new(12345);

    private static readonly TimeOnly Open  = new(9, 0);
    private static readonly TimeOnly Close = new(23, 0);

    public static async Task SeedAsync(MainDbContext db, ILogger logger, CancellationToken ct = default)
    {
        var hasAny = await db.Movies.AnyAsync(ct) || await db.Halls.AnyAsync(ct) ||
                     await db.ShowtimesSeries.AnyAsync(ct);
        if (hasAny)
        {
            logger.LogInformation("Данные уже есть, пропуск");
            return;
        }


        var genresAll = await db.Genres.AsNoTracking().ToListAsync(ct);

        var halls = CreateHalls();
        db.Halls.AddRange(halls);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Создано залов: {Count}", halls.Count);

        var movies = CreateMovies(genresAll);
        db.Movies.AddRange(movies);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Создано фильмов: {Count}", movies.Count);

        var activeMovies = movies.Where(m => m.IsRental).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var series = new List<ShowtimeSeries>();
        foreach (var hall in halls)
        {
            var pick = PickRandom(activeMovies, count: Rng.Next(4, 7));

            var timeCursor = Open;
            foreach (var movie in pick)
            {
                var endWithBreak = timeCursor.AddMinutes(movie.Duration + hall.TechBreak);
                if (endWithBreak > Close) break;

                var daysSpan = Rng.Next(7, 15);
                var from = today.AddDays(-Rng.Next(0, 3));
                var to = from.AddDays(daysSpan);

                var price = (int)RoundTo50(Rng.Next(250, 801));

                var s = ShowtimeSeries.Create(
                    id: Guid.NewGuid(),
                    movie: movie,
                    hall: hall,
                    startTime: timeCursor,
                    activeFrom: from,
                    activeTo: to,
                    basePrice: price,
                    status: ShowtimeStatus.Active
                );
                series.Add(s);

                timeCursor = endWithBreak;
            }
        }

        db.ShowtimesSeries.AddRange(series);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Создано серий сеансов: {Count}", series.Count);
    }

    private static List<Hall> CreateHalls()
    {
        var list = new List<Hall>();
        for (int i = 1; i <= 6; i++)
        {
            var id = Guid.NewGuid();

            var seats = Rng.Next(70, 201);   

            var breakMin = Rng.Next(10, 21);  

            var hall = Hall.Create(id, $"Зал {i}", seats, breakMin);
            
            list.Add(hall);
        }
        return list;
    }

    private static List<Movie> CreateMovies(List<Genre> allGenres)
    {
        var titles = new[]
        {
            "Сквозь время", "Последний рейс", "Тихий город", "Код тишины", "Осколки света",
            "Нулевой пациент", "Пятый континент", "Долина ветров", "Эхо глубины", "Край мира",
            "Шестое чувство юмора", "Праздник каждый день", "Ночной дозор", "Снег в июле",
            "Параллельные линии", "Граница сна", "Стеклянный дом", "Горячий лёд", "Город дождей",
            "Зона притяжения", "Лабиринт", "Две луны", "Восход тени", "Любовь и роботы", "Медный закат"
        };

        var ageRatings = new[] { "0+", "6+", "12+", "16+", "18+" };

        var list = new List<Movie>();

        foreach (var title in titles)
        {
            var id = Guid.NewGuid();
            var year = Rng.Next(2000, DateTime.UtcNow.Year + 1);
            var dur  = Rng.Next(85, 171); 
            var age  = ageRatings[Rng.Next(ageRatings.Length)];
            var isRental = Rng.NextDouble() > 0.15;

            var genreIds = PickRandom(allGenres, Rng.Next(1, 4)).Select(g => g.Id).ToArray();

            var m = Movie.Create(
                id: id,
                title: title,
                year: year,
                duration: dur,
                ageRating: age,
                isRental: isRental,
                posterKey: null,
                genreIds: genreIds
            );
            list.Add(m);
        }

        return list;
    }

    private static IEnumerable<T> PickRandom<T>(IReadOnlyList<T> src, int count)
    {
        if (count >= src.Count) return src;
        var idx = Enumerable.Range(0, src.Count).OrderBy(_ => Rng.Next()).Take(count);
        return idx.Select(i => src[i]).ToArray();
    }

    private static double RoundTo50(double x)
    {
        var rem = x % 50;
        return rem == 0 ? x : x + (50 - rem);
    }
}