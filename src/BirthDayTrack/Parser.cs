using System.Security.AccessControl;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BirthDayTrack;

internal static class Parser
{
    //Sets Solution's current directory to config/
    internal static string GetConfigSettingsPath()
    {
        string baseDir = Directory.GetCurrentDirectory();
        Console.WriteLine(Directory.GetCurrentDirectory());
        
        string appsettingsRelative = "config" + Path.DirectorySeparatorChar + "appsettings.json";

        string appsettingsPath = baseDir + Path.DirectorySeparatorChar + appsettingsRelative;
        Console.WriteLine(appsettingsPath);
        
        if (!Path.Exists(appsettingsPath))
        {
            appsettingsPath = Directory.GetParent(baseDir)!.Parent!.Parent!.Parent!.FullName +
                              Path.DirectorySeparatorChar + appsettingsRelative;
            Console.WriteLine(appsettingsPath);
        }

        return appsettingsPath;
    }

    internal static AppSettings? GetAppSettings()
    {
        string settPath = GetConfigSettingsPath();
        
        if (File.Exists(settPath))
        {
            string jsonContent = File.ReadAllText(settPath);
            
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            AppSettings? appSettings = JsonSerializer.Deserialize<AppSettings>(jsonContent, options);
            return appSettings;
        }
        else 
        {
            Console.WriteLine("appsettings.json was not found:");
        }

        return null;
    }

    
}