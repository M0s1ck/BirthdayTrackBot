using System.Security.AccessControl;
using System.Text.Json;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BirthDayTrack;

internal static class Parser
{
    //Sets Solution's current directory to config/
    internal static void SetCurrentDirectoryToConfig()
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        
        string defaultPath = Directory.GetCurrentDirectory();
        string[] defaultPathArr = defaultPath.Split(Path.DirectorySeparatorChar);
        string projectPath = string.Join(Path.DirectorySeparatorChar, defaultPathArr[..^5]);

        string configPath = Path.Combine(projectPath, "config");
        
        Directory.SetCurrentDirectory(configPath);
        Console.WriteLine(Directory.GetCurrentDirectory());
        
    }

    internal static AppSettings? GetAppSettings()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            
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