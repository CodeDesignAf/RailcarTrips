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
        var fallbackCsvPath = Path.Combine(contentRootPath, "SeedData", "cities.csv");
        var cities = new List<CityEntity>();

        if (!File.Exists(csvPath) && File.Exists(fallbackCsvPath))
        {
            csvPath = fallbackCsvPath;
        }

        if (File.Exists(csvPath))
        {
            var lines = await File.ReadAllLinesAsync(csvPath, cancellationToken);
            foreach (var rawLine in lines.Skip(1))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                var name = parts.Length >= 3 ? parts[1] : parts[0];
                var timeZone = parts.Length >= 3 ? parts[2] : parts[1];

                cities.Add(new CityEntity
                {
                    Name = name,
                    TimeZoneId = timeZone
                });
            }
        }

        if (cities.Count == 0)
        {
            cities.AddRange(
            [
                new CityEntity { Name = "Chicago", TimeZoneId = "Central Standard Time" },
                new CityEntity { Name = "Houston", TimeZoneId = "Central Standard Time" },
                new CityEntity { Name = "Denver", TimeZoneId = "Mountain Standard Time" },
                new CityEntity { Name = "Los Angeles", TimeZoneId = "Pacific Standard Time" },
                new CityEntity { Name = "New York", TimeZoneId = "Eastern Standard Time" }
            ]);
        }

        dbContext.Cities.AddRange(cities);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
