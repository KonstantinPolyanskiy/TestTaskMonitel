using Main.Application.Services.HallService.Models;
using Main.DAL.Database;
using Main.Domain.Hall;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Application.Services.HallService.Impl;

public sealed class HallService
{
    private readonly ILogger<HallService> _logger;
    private readonly MainDbContext _db;

    public HallService(MainDbContext db, ILogger<HallService> logger)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<Hall> CreateAsync(CreateHallModel req, CancellationToken ct = default)
    {
        var hallId = Guid.NewGuid();

        var hall = Hall.Create(
            id: hallId,
            name: req.Name,
            seats: req.Seats,
            techBreak: req.TechBreak
        );

        _db.Halls.Add(hall);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Создан зал {HallId} '{Title}' ({Seats})", hall.Id, hall.Name, hall.Seats);

        return hall;
    }

    public async Task<List<Hall>> GetAllAsync(CancellationToken ct = default)
    {
        var halls = await _db.Halls.AsNoTracking().ToListAsync(ct);

        return halls;
    }
}