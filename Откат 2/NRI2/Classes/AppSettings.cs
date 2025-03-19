using Microsoft.Extensions.Configuration;
using System.IO;

public class AppSettings
{
    public string AppName { get; set; }
    public string Version { get; set; }
    public int MaxLoginAttempts { get; set; }
}
public class FeatureFlags
{
    public bool EnableCache { get; set; }
    public bool EnableExperimentalUI { get; set; }
}
