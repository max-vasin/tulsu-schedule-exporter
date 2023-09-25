using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Serilog;

namespace ScheduleTools;

public class Portal
{
    static readonly Uri PortalUrl = new("https://tulsu.ru");
    
    public Portal()
    {
        Log.Information("portal built");
    }
    
    public async Task<ScheduleEntry[]> GetSchedule(string group)
    {
        var cookieContainer = new CookieContainer();
        using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        using var client = new HttpClient(handler) { BaseAddress = PortalUrl };
        
        var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new ("search_field", "GROUP_P"),
            new ("search_value", group)
        });
        
        cookieContainer.Add(PortalUrl, new Cookie("_ym_uid", "1690147955178438220"));
        cookieContainer.Add(PortalUrl, new Cookie("_ym_d", "1690147955"));
        cookieContainer.Add(PortalUrl, new Cookie("_ym_isad", "1"));
        
        var result = await client.PostAsync("/schedule/queries/GetSchedule.php", content);
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadAsStringAsync();
        var schedule = JsonConvert.DeserializeObject<ScheduleEntry[]>(response);

        if (schedule == null)
            throw new Exception("cannot deserialize response, its format, probably, was changed");

        return schedule;
    }
}