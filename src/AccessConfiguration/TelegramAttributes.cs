using Telegram.Bot;

namespace AccessConfiguration;

public static class TelegramAttributes
{
    public static ITelegramBotClient BotClient;
    public static CancellationToken Token;
}