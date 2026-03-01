using RailcarTrips.Application.Abstractions.Services;

namespace RailcarTrips.Infrastructure.Services;

public class TimeZoneConverter : ITimeZoneConverter
{
    public DateTime ConvertLocalToUtc(DateTime localDateTime, string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("Time zone id is required.", nameof(timeZoneId));
        }

        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
    }
}
