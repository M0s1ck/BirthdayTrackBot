using AccessConfiguration;
using Telegram.Bot;

namespace BirthDayTrack;

public static class Notifications
{
    //Присылает сообщения о дршках
    public static async Task Notify()
    {
        //Интервал 24 часа, ожидается запуск бота в 12 часов ночи, чтобы уведомления о наступивших дршках отправлялись в 12 
        TimeSpan interval = TimeSpan.FromHours(24);
        
        while (true)
        {
            List<string> birthdayBoys = await DataBaseQueries.GetBirthDayBoys(); //Именниники
            List<long> chatIds = await DataBaseQueries.GetAllChatIds(); //Всем пользователям отправится уведомление о др
            
            foreach (string user in birthdayBoys)
            {
                foreach (long chatId in chatIds)
                {
                    if (chatId != 0)
                    {
                        await TelegramAttributes.BotClient.SendTextMessageAsync(chatId, $"Поздравляем {user} с Днём Рождения!! \ud83e\udd73\ud83e\udd73");
                    }
                }
            }

            await Task.Delay(interval);
        }
    }
}