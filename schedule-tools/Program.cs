using System.Text;
using CommandLine;
using ScheduleTools;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Verbose()
    .CreateLogger();

Console.OutputEncoding = Encoding.UTF8;

try
{
    await Parser.Default.ParseArguments<Options>(args)
        .WithParsedAsync(async o =>
        {
            if (String.IsNullOrEmpty(o.Group))
                throw new Exception("you should provide a group number");

            var portal = new Portal();
            var schedule = await portal.GetSchedule(o.Group);

            foreach (var entry in schedule)
            {
                string discipline = entry.Discipline;
                
                Log.Information(discipline);
            }
        });
}
catch (Exception e)
{
    Log.Fatal(e.Message);
    return 1;
}

return 0;