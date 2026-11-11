using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TodoApp.Api.Data;

/// <summary>
/// Stores every <see cref="DateTime"/> in UTC (G-DB-4, Assumption 3) and marks values read back as UTC.
/// Local times are converted to UTC; unspecified times are treated as UTC already.
/// </summary>
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
