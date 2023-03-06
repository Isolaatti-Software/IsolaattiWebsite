using System.Collections.Generic;

namespace Isolaatti.Services;

public class ServerRenderedAlerts
{
    public Dictionary<string, string> Alerts { get; }

    public ServerRenderedAlerts()
    {
        Alerts = new Dictionary<string, string>();
    }
}