using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AccessConfiguration;

public class Response
{
    public Regex Request;
    public State State;
    private readonly string[] _respondTexts;
    private static readonly string NoAccessText = "\ud83d\ude10"; 

    public Response(string strRequest, State state, string[] respondTexts)
    {
        Request = new Regex(strRequest);
        State = state;
        _respondTexts = respondTexts;
    }

    public async Task BaseRespond(Message message, State state)
    {
        if (state != this.State)
        {
            await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, NoAccessText);
            return;
        }
        
        foreach (string respText  in _respondTexts)
        {
            await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, respText);   
        }
    }

    public bool CheckMatch(string input)
    {
        return Request.IsMatch(input);
    }
}