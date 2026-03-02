using RailcarTrips.Infrastructure.Data.Entities;

namespace RailcarTrips.Infrastructure.Data;

public class DbSeeder
{
    public async Task SeedCitiesIfEmptyAsync(
        AppDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Cities.Any())
        {
            return;
        }

        var csvPath = Path.Combine(contentRootPath, "SeedData", "canadian_cities.csv");
        var cities = new List<CityEntity>();

        // Seed only from canonical source file.
        if (!File.Exists(csvPath))
        {
            return;
        }

        var lines = await File.ReadAllLinesAsync(csvPath, cancellationToken);
        foreach (var rawLine in lines.Skip(1))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 3)
            {
                continue;
            }

            cities.Add(new CityEntity
            {
                Name = parts[1],
                TimeZoneId = parts[2]
            });
        }

        if (cities.Count == 0)
        {
            return;
        }

        dbContext.Cities.AddRange(cities);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
