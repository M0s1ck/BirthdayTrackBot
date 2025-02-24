using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace BirthDayTrack;

public static class Handlers
{
    //Принимает апдейты
    public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
        try
        {
            if (update.Type != UpdateType.Message)
            {
                Console.WriteLine("Wrong update type");
                return;
            }
                
            Message message = update.Message ?? new Message();
            User user = message.From ?? new User();
            Console.WriteLine($"{user.FirstName} ({user.Id}) написал {message.Text}");
                
            Chat chat = message.Chat;

            if (message.Type != MessageType.Text)
            {
                await botClient.SendTextMessageAsync(chat.Id, $"Используйте текстовые сообщения,а не {message.Type}", cancellationToken: cancellationToken);
                return;
            }
            
            // Тут выполняет команды в зависимости от содержания сообщений
            await Replies.Reply(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
        string errorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}