using CommandLine;

namespace ScheduleTools;

public class Options
{
    [Option(
        'g',
        "group",
        Required = true,
        HelpText = "Your student group number")]
    public string Group { get; set; } = String.Empty;
}