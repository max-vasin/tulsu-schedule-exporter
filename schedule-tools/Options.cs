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


    [Option(
        'o',
        "output",
        Required = true,
        HelpText = "Where to store iCalendar")]
    public string Output { get; set; } = String.Empty;
    
    
    [Option(
        'l',
        "language",
        Required = true,
        HelpText = "What is your foreign language (en,de,fr)")]
    public string Language { get; set; } = String.Empty;
    
    [Option(
        'c',
        "compare",
        Required = true,
        HelpText = "Compare with existing iCal")]
    public string Compare { get; set; } = String.Empty;
}