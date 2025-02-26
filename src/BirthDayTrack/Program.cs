using System.Collections;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using AccessConfiguration;
using DotNetEnv;

namespace BirthDayTrack;

internal static class Program
{
    static async Task Main()
    {
        MyBotUser mbu = new();
        Console.WriteLine(mbu.hey);

        DotNetEnv.Env.Load();
        
        string botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

        Formatter.Secret = botToken[^7..];
        
        string database = Environment.GetEnvironmentVariable("DB_NAME") ?? "localhost";
        string dbUser = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "birthdays";
        string dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "N / A";
        string host = Environment.GetEnvironmentVariable("DB_SERVER") ?? "N / A";
        
        DataBaseInfo dbInfo = new(host, database, dbUser, dbPass, pool: true, minPoolSize: 1, maxPoolSize: 10);
        
        string connectionString = Formatter.GetConnectionString(dbInfo);
        DataBaseQueries.ConnectionString = connectionString;
        
        // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
        ITelegramBotClient botClient = new TelegramBotClient(botToken); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
        
        // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
        ReceiverOptions receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
        {
            AllowedUpdates =
            [
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
            ],
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
            // True - не обрабатывать, False - обрабаывать
            ThrowPendingUpdates = true,
        };

        using CancellationTokenSource cts = new CancellationTokenSource();

        TelegramAttributes.BotClient = botClient;
        TelegramAttributes.Token = cts.Token;
        
        // UpdateHandler - обработчик приходящих Update`ов
        // ErrorHandler - обработчик ошибок, связанных с Bot API
        botClient.StartReceiving(Handlers.UpdateHandler, Handlers.ErrorHandler, receiverOptions, cts.Token); // Запускаем бота
        
        //Отправка уведомлений при дршках
        //TODO: await Notifications.Notify();

        User me = await botClient.GetMeAsync(cancellationToken: cts.Token); // Создаем переменную, в которую помещаем информацию о нашем боте.
        Console.WriteLine($"{me.FirstName} запущен!");

        await Task.Delay(-1, cts.Token); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
    }
}