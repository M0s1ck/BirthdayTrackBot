namespace BirthDayTrack;

public class AppSettings
{
    public string BotToken { get; set; }
    public DataBaseInfo DataBaseInfo { get; set; }
}

public class DataBaseInfo
{
     public string Server { get; set; }
     public string Database { get; set; }
     public string Uid { get; set; }
     public string Password { get; set; }
     public bool Pooling { get; set; }
     public int MinPoolSize { get; set; }
     public int MaxPoolSize { get; set; }
}