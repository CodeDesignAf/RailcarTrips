namespace RailcarTrips.Domain.Entities;

public class City
{
    public int Id { get; }
    public string Name { get; }
    public string TimeZoneId { get; }

    public City(int id, string name, string timeZoneId)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "City id must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("City name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("Time zone id is required.", nameof(timeZoneId));
        }

        Id = id;
        Name = name;
        TimeZoneId = timeZoneId;
    }
}
