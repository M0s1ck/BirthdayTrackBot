using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using AccessConfiguration;

namespace BirthDayTrack;

internal static class Program
{
    static async Task Main()
    {
        MyBotUser mbu = new();
        Console.WriteLine(mbu.hey);
        
        //Изменение рабочей директории
        Parser.SetCurrentDirectoryToConfig();
        
        //Парсинг данных с токеном и данных для подключения к бд. 
        AppSettings appSettings = Parser.GetAppSettings() ?? new AppSettings();
        
        string connectionString = Formatter.GetConnectionString(appSettings.DataBaseInfo);
        DataBaseQueries.ConnectionString = connectionString;
        
        string botToken = appSettings.BotToken ?? "";
        
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