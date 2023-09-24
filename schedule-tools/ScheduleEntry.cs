using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ScheduleTools;

public class TimeRange
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
}

public class TimeRangeConverter : JsonConverter<TimeRange>
{
    public override bool CanWrite => false;
    public override bool CanRead => true;

    public override void WriteJson(JsonWriter writer, TimeRange? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override TimeRange? ReadJson(
        JsonReader reader,
        Type objectType,
        TimeRange? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var source = reader.Value?.ToString();
        if (!string.IsNullOrEmpty(source))
        {
            var parts = source
                .Split("-")
                .Select(x => x.Trim())
                .ToArray();

            if (parts.Length != 2)
                throw new Exception($"cant convert string to time range {source}");


            return new TimeRange()
            {
                Start = TimeOnly.Parse(parts[0]),
                End = TimeOnly.Parse(parts[1])
            };
        }

        return null;
    }
}

public class TulsuDateTimeConverter : JsonConverter<DateTime>
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override DateTime ReadJson(
        JsonReader reader,
        Type objectType,
        DateTime existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var source = reader.Value?.ToString();
        if (!string.IsNullOrEmpty(source))
        {

            var parts = source
                .Split(".")
                .Select(x => x.Trim())
                .ToArray();

            if (parts.Length != 3)
                throw new Exception($"cant convert string to date time {source}");

            var day = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var year = int.Parse(parts[2]);
            
            return new DateTime(year, month, day);
        }

        throw new Exception("missed date time value");
    }
}

public class GroupEntry
{
    [JsonProperty("GROUP_P")]
    public string Group { get; set; } = String.Empty;
}

public class ScheduleEntry
{
    [JsonProperty("DATE_Z")]
    [JsonConverter(typeof(TulsuDateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonProperty("TIME_Z")]
    [JsonConverter(typeof(TimeRangeConverter))]
    public TimeRange Time { get; set; }

    [JsonProperty("DISCIP")]
    public string Discipline { get; set; } = String.Empty;

    [JsonProperty("KOW")]
    public string Format { get; set; } = String.Empty;

    [JsonProperty("AUD")]
    public string Audience { get; set; } = String.Empty;

    [JsonProperty("PREP")]
    public string Speaker { get; set; } = String.Empty;

    [JsonProperty("CLASS")]
    public string Class { get; set; } = String.Empty;

    [JsonProperty("GROUPS")]
    public GroupEntry[] Groups { get; set; } = { };
}