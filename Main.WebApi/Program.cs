using System.Globalization;
using Main.Application.Contracts;
using Main.Application.Services.BrowseService.Impl;
using Main.Application.Services.HallService.Impl;
using Main.Application.Services.MovieService.Impl;
using Main.Application.Services.ShowtimeSeries.Impl;
using Main.DAL.Database;
using Main.DAL.Database.Seeds;
using Main.DAL.Minio;
using Main.Domain.Cinema;
using Main.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Minio;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MinioSettings>()
    .Bind(builder.Configuration.GetSection("Minio"));

builder.Services
    .AddOptions<CinemaHoursOptions>()
    .Bind(builder.Configuration.GetSection("CinemaHours"))
    .Validate(o =>
    {
        if (!TimeOnly.TryParseExact(o.Open, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var open))
            return false;
        if (!TimeOnly.TryParseExact(o.Close, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var close))
            return false;
        return open < close;
    }, "CinemaHours: укажи время в формате HH:mm и чтобы Open < Close")
    .ValidateOnStart();


builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<MinioSettings>>().Value;

    return new MinioClient()
        .WithEndpoint(opts.Endpoint)                  
        .WithCredentials(opts.AccessKey, opts.SecretKey)
        .WithSSL(opts.IsSecure)
        .Build();
});

builder.Services.AddDbContext<MainDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("PostgresDb");
    opt.UseNpgsql(cs);
});

builder.Services.AddScoped<IFileStorage, MinioStorage>();

builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<HallService>();
builder.Services.AddScoped<ShowtimeSeriesService>();
builder.Services.AddScoped<BrowseService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Кинотеатра", Version = "v1" });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db  = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DevDataSeeder");

    await db.Database.MigrateAsync();
    await DevDataSeeder.SeedAsync(db, log);
}

app.UseMiddleware<ExceptionMappingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cinema API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();
app.Run();
