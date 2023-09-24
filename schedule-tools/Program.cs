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
            
            
            var exp = new Exporter(o.Output, o.Language);
            exp.Export(schedule);
            return;

            /*
            if (!string.IsNullOrEmpty(o.Compare))
            {
                var exporter = new Exporter(o.Compare);
                exporter.Compare(schedule);
            }
            else
            {
                var exporter = new Exporter(o.Output);
                exporter.Export(schedule);
            }
            */
        });
}
catch (Exception e)
{
    Log.Fatal(e.Message);
    return 1;
}

return 0;