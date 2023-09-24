using System.Text;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Serilog;

namespace ScheduleTools;

public class Exporter
{
    static readonly Dictionary<string, string> languageEntry = new()
    {
        ["en"] = "английский",
        ["de"] = "немецкий",
        ["fr"] = "французский"
    };

    readonly string _fileName;
    readonly string _langEntry;

    public Exporter(string fileName, string language)
    {
        _fileName = fileName;
        _langEntry = languageEntry[language];
    }

    bool LanguageFilter(ScheduleEntry entry)
    {
        if (entry.Discipline == "Иностранный язык")
            return entry.Format.Contains(_langEntry);

        return true;
    }

    ScheduleEntry[] MapSchedule(IEnumerable<ScheduleEntry> schedule)
    {
        var byDates = schedule
            .Where(LanguageFilter)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Time.Start)
            .GroupBy(e => e.Date)
            .ToArray();

        foreach (var byDate in byDates)
        {
            foreach (var duplicate in byDate.GroupBy(e => e.Discipline))
            {
                if (duplicate.Count() > 1)
                {
                    int index = 0;
                    foreach (var entry in duplicate)
                    {
                        var fixedName = $"{entry.Discipline} #{++index}";
                        Log.Information("fixing discipline to {s}", fixedName);
                        entry.Discipline = fixedName;
                    }
                }
            }
        }

        return byDates.SelectMany(e => e).ToArray();
    }

    public void Export(ICollection<ScheduleEntry> schedule)
    {
        Log.Information("mapping schedule");

        var mappedSchedule = MapSchedule(schedule);

        Log.Information("{en} of {in} entries mapped, begin calendar export", mappedSchedule.Length, schedule.Count());

        var events = MapSchedule(mappedSchedule)
            .Select(x => new CalendarEvent()
            {
                Start = new CalDateTime(x.Date + x.Time.Start.ToTimeSpan()),
                End = new CalDateTime(x.Date + x.Time.End.ToTimeSpan()),
                Name = "VEVENT",
                Location = x.Audience,
                Status = "CONFIRMED",
                Sequence = 0,
                Summary = string.IsNullOrEmpty(x.Speaker)
                    ? $"!!! {x.Discipline}: нет препода"
                    : $"{x.Discipline}: {x.Speaker}",
                Description = x.Format,
                Uid = x.Date.ToShortDateString() + x.Discipline,
            })
            .ToArray();

        var calendar = new Calendar();
        calendar.Events.AddRange(events);

        var serializer = new CalendarSerializer();
        var content = serializer.SerializeToString(calendar);

        using var stream = File.OpenWrite(_fileName);
        using var writer = new StreamWriter(stream);

        writer.Write(content);

        Log.Information("calendar {e} events exported to {f}", events.Count(), _fileName);
    }

    public void Compare(IEnumerable<ScheduleEntry> schedule)
    {
        using var stream = File.OpenRead(_fileName);
        //using var reader = new StreamReader(stream);

        //var content = reader.ReadToEnd();

        var calendar = Calendar.Load(stream);

        //using var reader = new 
        //  var serializer = new CalendarSerializer();
        //   var calendar = (Calendar)serializer.Deserialize(stream, Encoding.UTF8);

        var mapped = calendar.Events
            .Select(e =>
            {
                var parts = e.Summary.Split(":")
                    .Select(x => x.Replace("!!!", "").Trim())
                    .ToArray();

                return new ScheduleEntry()
                {
                    Audience = e.Location,
                    Discipline = parts[0],
                    Speaker = parts[1],
                    Format = e.Description,
                    Date = e.Start.Date
                };
            })
            .ToArray();

        Log.Information("loaded {e} events, in current schedule {c}", mapped.Count(), schedule.Count());

        var diff = schedule
            .Select(e => new { e.Date, e.Discipline, e.Speaker })
            .Except(mapped.Select(e => new { e.Date, e.Discipline, e.Speaker }))
            .OrderBy(e => e.Date);


        foreach (var exceed in diff)
            Log.Warning("{a} {b} {c}", exceed.Date, exceed.Discipline, exceed.Speaker);
    }
}