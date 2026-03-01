namespace RailcarTrips.Application.Abstractions.Services;

public interface ITimeZoneConverter
{
    DateTime ConvertLocalToUtc(DateTime localDateTime, string timeZoneId);
}
