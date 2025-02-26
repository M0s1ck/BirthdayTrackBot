using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AccessConfiguration;

namespace BirthDayTrack;

/// <summary>
/// Вспомогательный класс для форматирования данных
/// </summary>
internal static class Formatter
{
    internal static readonly Regex DateForm = new Regex(@"(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])");
    
    internal static string GetConnectionString(DataBaseInfo dataBaseInfo)
    {
        string server = dataBaseInfo.Server;
        string db = dataBaseInfo.Database;
        string uid = dataBaseInfo.Uid;
        string password = dataBaseInfo.Password;
        bool pooling = dataBaseInfo.Pooling;
        int minPoolsize = dataBaseInfo.MinPoolSize;
        int maxPoolsize = dataBaseInfo.MaxPoolSize;
        return $"Server={server};Database={db};Uid={uid};Password={password};Pooling={pooling.ToString().ToLower()};MinPoolSize={minPoolsize};MaxPoolSize={maxPoolsize};";
    }
    
    internal static string FormatUsers(IOrderedEnumerable<Tuple<string, DateOnly>> users)
    {
        StringBuilder sbd = new();
        foreach (Tuple<string, DateOnly> tup in users)
        {
            string name = tup.Item1;
            DateOnly date = tup.Item2;
            sbd.Append($"{name} - {date.Day} {Months[date.Month]}{Environment.NewLine}");
        }

        return sbd.ToString();
    }

    internal static bool IsValidDate(string dateText)
    {
        return DateTime.TryParseExact(dateText, "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);  
    }
    
    private static readonly Dictionary<int, string> Months = new Dictionary<int, string>
    {
        { 1, "января" },
        { 2, "февраля" },
        { 3, "марта" },
        { 4, "апреля" },
        { 5, "мая" },
        { 6, "июня" },
        { 7, "июля" },
        { 8, "августа" },
        { 9, "сентября" },
        { 10, "октября" },
        { 11, "ноября" },
        { 12, "декабря" }
    };
    
    
    internal static readonly Dictionary<string, string> TextReplies = new Dictionary<string, string>
    {
        {
            "/start", //"Привет! Этот бот может напоминать о др.\nЧтобы зарегаться, напиши дату своего рождения\n(формат - dd.mm, например 07.06 или 25.10)"
            "Привет! Этот бот может напоминать о др.\n Должно быть, тебе приходило приглашение с кодом, введи его \ud83d\ude42"
        },
        {
            "Изменить моё др",
            "Вы уверены, что хотите изменить своё др?"
        },
        {
            "Нет",
            "Ну хорошо"
        },
        {
            "Удалить меня из системы",
            "Вы уверены, что хотите, чтобы вас удалили из системы?\n(Вы не сможете увидеть др других людей и получать о них уведомления)"
        },
        {
            "Пока",
            "Пока 😳"
        }
    };

    
    internal static readonly Dictionary<State, string> ModifyReplies = new Dictionary<State, string>
    {
        {
            State.Updating,
            "Введите новую дату (формат - dd.mm, например 07.06 или 25.10)"
        },
        {
            State.Deleting,
            "Вы были удалены из системы, другие пользователи не увидят ваше др \ud83d\ude2d\n" +
            "(Если хотите зарегаться заново, введите /start)"
        }
    };
    
    internal static string Secret { get; set; }
}