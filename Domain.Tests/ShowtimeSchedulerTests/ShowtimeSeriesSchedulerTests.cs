using FluentAssertions;
using Main.Domain.Cinema;
using Main.Domain.Exceptions;
using Main.Domain.Hall;
using Main.Domain.Movie;
using Main.Domain.Session;

namespace Domain.Tests.ShowtimeSchedulerTests;

public sealed class ShowtimeSchedulerTests
{
    private static readonly DateOnly D = new(2025, 10, 06);
    private static readonly BusinessHours Hours = new(new TimeOnly(9, 0), new TimeOnly(23, 0));

    [Fact]
    public void EnsureCanSchedule_Valid_NoExisting_ShouldNotThrow()
    {
        var movie = NewMovie(duration: 100, isRental: true);
        var hall  = NewHall(techBreak: 10);

        var candidate = NewSeries(
            movie, 
            hall,
            new TimeOnly(10, 0), 
            D, 
            D.AddDays(7), 
            400,
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate,
            movie,
            hall,
            Hours,
            existingSeries: Array.Empty<ShowtimeSeries>(),
            moviesById: Dict(movie),
            validateFrom: D, 
            validateTo: D.AddDays(7));

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanSchedule_ValidateToBeforeFrom_ShouldThrowConflict()
    {
        var movie = NewMovie(100, true);
        var hall  = NewHall(10);

        var candidate = NewSeries(
            movie,
            hall, new TimeOnly(10, 0),
            D, 
            D.AddDays(1), 
            300, 
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate,
            movie, 
            hall,
            Hours,
            existingSeries: Array.Empty<ShowtimeSeries>(),
            moviesById: Dict(movie),
            validateFrom: D.AddDays(2), validateTo: D.AddDays(1));

        act.Should().Throw<DomainConflictException>();
    }

    [Fact]
    public void EnsureCanSchedule_CandidateCancelled_ShouldNotThrow_EvenIfWouldOverlap()
    {
        var movie = NewMovie(100, true);
        var hall  = NewHall(10);

        var existing = NewSeries(
            movie,
            hall,
            new TimeOnly(10, 0),
            D,
            D,
            300,
            ShowtimeStatus.Active);

        var candidate = NewSeries(
            movie,
            hall,
            new TimeOnly(10, 30),
            D,
            D,
            300,
            ShowtimeStatus.Cancelled);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate,
            movie,
            hall,
            Hours,
            existingSeries: [existing],
            moviesById: Dict(movie),
            validateFrom: D,
            validateTo: D);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanSchedule_ExceedsBusinessHours_ShouldThrowConflict()
    {
        var movie = NewMovie(duration: 80, isRental: true);
        var hall = NewHall(techBreak: 10);

        var candidate = NewSeries(
            movie,
            hall, 
            new TimeOnly(22, 30), 
            D, 
            D, 
            300,
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate, 
            movie, 
            hall,
            Hours,
            existingSeries: Array.Empty<ShowtimeSeries>(),
            moviesById: Dict(movie),
            validateFrom: D,
            validateTo: D);

        act.Should().Throw<DomainConflictException>();
    }

    [Fact]
    public void EnsureCanSchedule_OverlapSameHall_ShouldThrowConflict()
    {
        var movie = NewMovie(120, true);
        var hall  = NewHall(10);

        var existing = NewSeries(
            movie, 
            hall, 
            new TimeOnly(10, 0), 
            D, 
            D,
            300,
            ShowtimeStatus.Active);

        var candidate = NewSeries(
            movie,
            hall,
            new TimeOnly(11, 0), 
            D, 
            D, 
            300, 
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate,
            movie, 
            hall,
            Hours,
            existingSeries: [existing],
            moviesById: Dict(movie),
            validateFrom: D,
            validateTo: D);

        act.Should().Throw<DomainConflictException>();
    }

    [Fact]
    public void EnsureCanSchedule_TouchingEdges_NoOverlap_ShouldNotThrow()
    {
        var movie = NewMovie(60, true);
        var hall  = NewHall(10);

        var existing = NewSeries(
            movie,
            hall,
            new TimeOnly(10, 0), 
            D, 
            D, 
            300,
            ShowtimeStatus.Active);

        var candidate = NewSeries(
            movie, 
            hall,
            new TimeOnly(11, 10),
            D, 
            D, 
            300,
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate, 
            movie,
            hall,
            Hours,
            existingSeries: [existing],
            moviesById: Dict(movie),
            validateFrom: D, 
            validateTo: D);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanSchedule_DifferentHall_ShouldNotThrow()
    {
        var movie = NewMovie(100, true);
        var hallA = NewHall(10);
        var hallB = NewHall(10);

        var existing = NewSeries(
            movie, 
            hallA,
            new TimeOnly(10, 0),
            D, 
            D, 
            300,
            ShowtimeStatus.Active);

        var candidate = NewSeries(
            movie,
            hallB, 
            new TimeOnly(10, 30), 
            D, 
            D, 
            300,
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate, 
            movie, 
            hallB,
            Hours,
            existingSeries: [existing],
            moviesById: Dict(movie),
            validateFrom: D, 
            validateTo: D);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanSchedule_ExistingCancelled_ShouldNotThrow()
    {
        var movie = NewMovie(100, true);
        var hall  = NewHall(10);

        var existing = NewSeries(
            movie, 
            hall, 
            new TimeOnly(10, 0),
            D, 
            D, 
            300, 
            ShowtimeStatus.Cancelled);

        var candidate = NewSeries(
            movie, 
            hall, 
            new TimeOnly(10, 30),
            D, 
            D, 
            300, 
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate, 
            movie, 
            hall, 
            Hours,
            existingSeries: [existing],
            moviesById: Dict(movie),
            validateFrom: D,
            validateTo: D);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanSchedule_ExistingInactiveOnDay_ShouldNotThrow()
    {
        var movie = NewMovie(100, true);
        var hall  = NewHall(10);

        var existing = NewSeries(
            movie, 
            hall, 
            new TimeOnly(10, 0),
            D.AddDays(-1),
            D.AddDays(-1), 
            300,
            ShowtimeStatus.Active);

        var candidate = NewSeries(
            movie,
            hall,
            new TimeOnly(10, 0), 
            D, 
            D, 
            300, 
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate, 
            movie, 
            hall,
            Hours,
            existingSeries: [existing],
            moviesById: Dict(movie),
            validateFrom: D,
            validateTo: D);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureCanSchedule_NoMovieInDictionary_ShouldThrowNotFound()
    {
        var movie = NewMovie(100, true);
        var hall  = NewHall(10);

        var otherMovie = NewMovie(120, true);
        var existing = NewSeries(
            otherMovie, 
            hall, 
            new TimeOnly(10, 0), 
            D, 
            D, 
            300, 
            ShowtimeStatus.Active);

        var candidate = NewSeries(
            movie, 
            hall, 
            new TimeOnly(10, 30), 
            D, 
            D, 
            300, 
            ShowtimeStatus.Active);

        var moviesById = Dict(movie);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate,
            movie, 
            hall, 
            Hours,
            existingSeries: [existing],
            moviesById: moviesById,
            validateFrom: D,
            validateTo: D);

        act.Should().Throw<DomainNotFoundException>();
    }

    [Fact]
    public void EnsureCanSchedule_NoIntersectionWithValidationWindow_ShouldNotThrow()
    {
        var movie = NewMovie(120, true);
        var hall  = NewHall(10);

        var existing = NewSeries(
            movie, 
            hall, 
            new TimeOnly(10, 0),
            D, 
            D.AddDays(3),
            300, 
            ShowtimeStatus.Active);

        var candidate = NewSeries(
            movie, 
            hall, 
            new TimeOnly(10, 30),
            D, 
            D.AddDays(3),
            300, 
            ShowtimeStatus.Active);

        var act = () => ShowtimeScheduler.EnsureCanSchedule(
            candidate, 
            movie, 
            hall, 
            Hours,
            existingSeries: [existing],
            moviesById: Dict(movie),
            validateFrom: D.AddDays(10), 
            validateTo: D.AddDays(12));

        act.Should().NotThrow();
    }

    private static Movie NewMovie(int duration, bool isRental)
    {
        var id = Guid.NewGuid();
        var year = Math.Min(DateTime.UtcNow.Year, 2025);

        return Movie.Create(
            id: id,
            title: $"Movie-{id.ToString()[..4]}",
            year: year,
            duration: duration,
            ageRating: "12+",
            isRental: isRental,
            posterKey: null,
            genreIds: new[] { 1 }
        );
    }

    private static Hall NewHall(int techBreak)
    {
        var id = Guid.NewGuid();
        return Hall.Create(id, $"Hall-{id.ToString()[..4]}", seats: 100, techBreak: techBreak);
    }

    private static ShowtimeSeries NewSeries(
        Movie movie, Hall hall, TimeOnly start, DateOnly from, DateOnly to, int price, ShowtimeStatus status)
    {
        return ShowtimeSeries.Create(
            id: Guid.NewGuid(),
            movie: movie,
            hall: hall,
            startTime: start,
            activeFrom: from,
            activeTo: to,
            basePrice: price,
            status: status
        );
    }

    private static IReadOnlyDictionary<Guid, Movie> Dict(params Movie[] movies)
        => movies.ToDictionary(m => m.Id, m => m);
}