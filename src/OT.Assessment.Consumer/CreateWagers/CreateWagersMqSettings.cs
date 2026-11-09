namespace OT.Assessment.Consumer.CreateWagers;

public class CreateWagersMqSettings
{
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string ErrorQueue { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = string.Empty;
    public ushort PrefetchCount { get; set; }
}
