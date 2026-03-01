using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RailcarTrips.Application.Abstractions.Persistence;
using RailcarTrips.Application.Abstractions.Services;
using RailcarTrips.Infrastructure.Data;
using RailcarTrips.Infrastructure.Repositories;
using RailcarTrips.Infrastructure.Services;

namespace RailcarTrips.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["Data:ConnectionString"] ?? "Data Source=railcartrips.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IEquipmentEventRepository, EquipmentEventRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<ICsvEquipmentEventReader, CsvEquipmentEventReader>();
        services.AddScoped<ITimeZoneConverter, TimeZoneConverter>();
        services.AddScoped<DbSeeder>();

        return services;
    }
}
