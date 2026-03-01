using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using RailcarTrips.Application.Abstractions.Services;
using RailcarTrips.Application.DTOs;
using System.Globalization;

namespace RailcarTrips.Infrastructure.Services;

public class CsvEquipmentEventReader : ICsvEquipmentEventReader
{
    public async Task<IReadOnlyList<CsvEquipmentEventRowDto>> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            PrepareHeaderForMatch = args => args.Header.Trim().Replace(" ", string.Empty).ToLowerInvariant(),
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<CsvRowMap>();
        var rows = new List<CsvEquipmentEventRowDto>();

        try
        {
            await foreach (var record in csv.GetRecordsAsync<CsvRow>(cancellationToken))
            {
                rows.Add(new CsvEquipmentEventRowDto(
                    record.EquipmentId,
                    record.CityId,
                    record.EventCode,
                    record.EventTime));
            }
        }
        catch (ReaderException ex)
        {
            throw new InvalidDataException(
                "CSV format is invalid. Expected headers: Equipment Id, Event Code, Event Time, City Id.",
                ex);
        }
        catch (TypeConverterException ex)
        {
            throw new InvalidDataException("CSV contains invalid values.", ex);
        }

        return rows;
    }

    private sealed class CsvRow
    {
        public string EquipmentId { get; init; } = default!;
        public string EventCode { get; init; } = default!;
        public string EventTime { get; init; } = default!;
        public string CityId { get; init; } = default!;
    }

    private sealed class CsvRowMap : ClassMap<CsvRow>
    {
        public CsvRowMap()
        {
            Map(m => m.EquipmentId).Name("Equipment Id");
            Map(m => m.EventCode).Name("Event Code");
            Map(m => m.EventTime).Name("Event Time");
            Map(m => m.CityId).Name("City Id");
        }
    }
}
