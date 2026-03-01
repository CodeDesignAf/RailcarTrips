using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using RailcarTrips.Application.Abstractions.Persistence;
using RailcarTrips.Application.Abstractions.Services;
using RailcarTrips.Application.DTOs;
using RailcarTrips.Domain.Entities;
using RailcarTrips.Domain.Enums;
using RailcarTrips.Domain.Services;

namespace RailcarTrips.Application.UseCases;

public class ImportEquipmentEventsUseCase
{
    private readonly ICsvEquipmentEventReader _csvReader;
    private readonly ITimeZoneConverter _timeZoneConverter;
    private readonly ICityRepository _cityRepository;
    private readonly IEquipmentEventRepository _eventRepository;
    private readonly ITripRepository _tripRepository;
    private readonly TripProcessingEngine _engine;

    public ImportEquipmentEventsUseCase(
        ICsvEquipmentEventReader csvReader,
        ITimeZoneConverter timeZoneConverter,
        ICityRepository cityRepository,
        IEquipmentEventRepository eventRepository,
        ITripRepository tripRepository)
    {
        _csvReader = csvReader;
        _timeZoneConverter = timeZoneConverter;
        _cityRepository = cityRepository;
        _eventRepository = eventRepository;
        _tripRepository = tripRepository;
        _engine = new TripProcessingEngine();
    }

    public async Task<ImportResultDto> ExecuteAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var rows = await _csvReader.ReadAsync(csvStream, cancellationToken);
        var anomalies = new List<string>();
        const int maxAnomalyMessages = 50;
        var anomaliesCount = 0;
        var recordsToPersist = new List<PersistableEquipmentEventDto>();
        var ignoredEventsCount = 0;
        var ignoredEventCodesSummary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.EquipmentId))
            {
                continue;
            }

            if (!int.TryParse(row.CityId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cityId))
            {
                continue;
            }

            var city = await _cityRepository.GetByIdAsync(cityId, cancellationToken);
            if (city is null)
            {
                continue;
            }

            var eventCode = MapEventCode(row.EventCode);
            if (eventCode == EventCode.Other)
            {
                ignoredEventsCount++;
                var normalized = (row.EventCode ?? string.Empty).Trim().ToUpperInvariant();
                var key = string.IsNullOrWhiteSpace(normalized) ? "EMPTY" : normalized;
                if (!ignoredEventCodesSummary.TryAdd(key, 1))
                {
                    ignoredEventCodesSummary[key]++;
                }
            }

            if (!DateTime.TryParse(row.EventTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var localDateTime))
            {
                continue;
            }

            localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);

            DateTime utcDateTime;
            try
            {
                utcDateTime = _timeZoneConverter.ConvertLocalToUtc(localDateTime, city.TimeZoneId);
            }
            catch (Exception)
            {
                continue;
            }

            var naturalKeyHash = ComputeNaturalKeyHash(row.EquipmentId, city.Id, eventCode, utcDateTime);

            recordsToPersist.Add(new PersistableEquipmentEventDto(
                row.EquipmentId.Trim(),
                city.Id,
                eventCode.ToString(),
                utcDateTime,
                localDateTime,
                naturalKeyHash));
        }

        var saveResult = await _eventRepository.AddRangeIgnoringDuplicatesAsync(recordsToPersist, cancellationToken);

        var tripsCreated = 0;
        var tripsClosed = 0;

        foreach (var equipmentId in saveResult.AffectedEquipmentIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var persistedEvents = await _eventRepository.GetByEquipmentIdAsync(equipmentId, cancellationToken);
            var domainEvents = persistedEvents
                .Select(e => new EquipmentEvent(
                    e.Id,
                    e.EquipmentId,
                    e.CityId,
                    ParseDomainEventCode(e.EventCode),
                    e.EventUtcTime,
                    e.EventLocalTime))
                .ToArray();

            var processingResult = _engine.Process(domainEvents);
            await _tripRepository.ReplaceForEquipmentAsync(equipmentId, processingResult.Trips, cancellationToken);

            tripsCreated += processingResult.Trips.Count;
            tripsClosed += processingResult.Trips.Count(t => t.Status == TripStatus.Closed);
            anomaliesCount += processingResult.AnomaliesCount;
            foreach (var anomaly in processingResult.Anomalies)
            {
                if (anomalies.Count >= maxAnomalyMessages)
                {
                    break;
                }

                anomalies.Add(anomaly);
            }
        }

        return new ImportResultDto(
            saveResult.Inserted,
            saveResult.Duplicates,
            tripsCreated,
            tripsClosed,
            ignoredEventsCount,
            ignoredEventCodesSummary,
            anomaliesCount,
            anomalies);
    }

    private static EventCode MapEventCode(string? rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
        {
            return EventCode.Other;
        }

        var normalized = rawCode.Trim().ToUpperInvariant();
        if (normalized == "W")
        {
            return EventCode.ReleasedW;
        }

        if (normalized == "Z")
        {
            return EventCode.PlacedZ;
        }

        return EventCode.Other;
    }

    private static string ComputeNaturalKeyHash(
        string equipmentId,
        int cityId,
        EventCode eventCode,
        DateTime eventUtcTime)
    {
        var payload = $"{equipmentId.Trim().ToUpperInvariant()}|{cityId}|{eventCode}|{eventUtcTime:O}";
        var bytes = Encoding.UTF8.GetBytes(payload);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private static EventCode ParseDomainEventCode(string rawCode)
    {
        // TODO: Replace with centralized mapper once event-code translation rules evolve.
        return Enum.TryParse<EventCode>(rawCode, true, out var parsed)
            ? parsed
            : EventCode.Other;
    }
}
