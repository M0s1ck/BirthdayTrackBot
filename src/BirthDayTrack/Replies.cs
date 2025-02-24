using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using System.Text.RegularExpressions;
using AccessConfiguration;
using Telegram.Bot.Types.Enums;

namespace BirthDayTrack;

/// <summary>
/// Тут содержатся "ответы" бота на присылаемые сообщения, то есть что бот сделает зависимости от них 
/// </summary>
public static class Replies
{
        
    //Тут описана основная логика 
    private static readonly Dictionary<Regex, Func<Message, Task>> MessageReplies =
        new Dictionary<Regex, Func<Message, Task>>
        {
            {
                new Regex("/start"),
                Start
            },
            {
                new Regex(@"\d{9}"),
                GivenCode
            },
            {
                Formatter.DateForm,
                GivenDate
            },
            {
                new Regex("Показать все дршки"),
                ShowUsers
            },
            {
                new Regex("Отправить приглашение другу"),
                Invite
            },
            {
                new Regex("Изменить моё др"), 
                MaybeUpdateBirthday
            },
            {
                new Regex("Удалить меня из системы"), 
                MaybeDeleteBd      
            },
            {
                new Regex("Да"),  //change
                Modify
            },
            {
                new Regex("Нет"),
                CancelModifications
            },
            {
                new Regex("Привет"),
                Greet
            },
            {
                new Regex("Пока"),
                BaseReply__
            }
        };

    public static List<Response> Responses =
    [
        new Response("/start", State.EnteringBirthday, ["Привет! Этот бот может напоминать о др.\n Должно быть, тебе приходило приглашение с кодом, введи его \ud83d\ude42"])
    ];
    
    //Сопоставляет содержание сообщения с выполняемым функционалом из делегата выше
    internal static async Task Reply(Message message)
    {
        foreach (var keyVal in MessageReplies)
        {
            Regex regex = keyVal.Key;
            if (regex.IsMatch(message.Text ?? ""))
            {
                Func<Message, Task> func = keyVal.Value;
                await func(message);
                return;
            }
        }
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id,
            "\ud83d\ude10", cancellationToken: TelegramAttributes.Token);
    }
        
    
    private static async Task BaseReply__(Message message)
    {
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, Formatter.TextReplies[message.Text ?? ""],
            cancellationToken: TelegramAttributes.Token);
    }

    
    private static async Task KeyBoardReply__(Message message, ReplyKeyboardMarkup? markup)
    {
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, Formatter.TextReplies[message.Text ?? ""],
            replyMarkup: markup, cancellationToken: TelegramAttributes.Token);
    }

    
    private static async Task Start(Message message)
    {
        
        await BaseReply__(message);
        string invUsername = message.From.Username;
        
    }

    //TODO: Обработка невалидного кода
    private static async Task GivenCode(Message message)
    {
        int invCode = int.Parse(message.Text ?? "");
        string? roomName = await DataBaseQueries.GetRoomByInvCode(invCode);
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, roomName ?? "null");
        
        if (roomName == null)
        {
            await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, "Код не валидный");
            return;
        }

        string username = message.From.Username;
        long chatId = message.Chat.Id;
        
        await DataBaseQueries.AddInvitedUser(username, chatId, roomName);
        
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, "Отлично! Теперь напишите дату своего рождения\n(формат - dd.mm, например 07.06 или 25.10)");
    }

    
    //Последует либо добавление пользователя, либо изменение его др
    private static async Task GivenDate(Message message)
    {
        string dateText = message.Text ?? "";
        if (!Formatter.IsValidDate(dateText))
        {
            await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id,
                "Неверный формат даты", cancellationToken: TelegramAttributes.Token);
            return;
        }
        
        //TODO: Разгрести: нет условности, просто ввод др

        string username = message.From.Username;
        
        State previousState = await DataBaseQueries.GetState(username);
        
        await DataBaseQueries.SetBirthday(username, message.Text ?? "");
        
        //Отправка сообщений
        switch (previousState)
        {
            case State.EnteringBirthday:
            {
                await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id,
                    "Отлично, вы были успешно зареганы!!", cancellationToken: TelegramAttributes.Token);

                await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id,
                    "Теперь вы можете посмотреть список своих друзей и многое другое",
                    replyMarkup: GetAddedUserKeyBoard(), cancellationToken: TelegramAttributes.Token);
                return;
            }
            case State.Updating:
            {
                await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, "Ваше др было изменено",
                    replyMarkup: GetAddedUserKeyBoard(), cancellationToken: TelegramAttributes.Token);
                return;
            }
            default:
            {
                await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id,
                    "\ud83d\ude10", cancellationToken: TelegramAttributes.Token);
                return;
            }
        }
    }

    
    private static async Task Greet(Message message)
    {
        string text = $"Привет!! @{message.From.Username} {message.From.FirstName} {message.From.LastName}"; 
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, text, cancellationToken: TelegramAttributes.Token);
    }

    
    private static async Task ShowUsers(Message message)
    {
        string answer = Formatter.FormatUsers(await DataBaseQueries.GetAllUsers());
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, answer, cancellationToken: TelegramAttributes.Token);
    }

    private static async Task Invite(Message message)
    {
        string invitersUsername = message.From.Username ?? "";
        string roomName = await DataBaseQueries.GetUsersRoom(invitersUsername);
        
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, roomName, cancellationToken: TelegramAttributes.Token);
        
        Room usersRoom = RoomsData.GetRoomByName(roomName);
        int invitationId = usersRoom.GetInvitationId();
        string invitationText = usersRoom.GetInvitationMessage(invitationId);

        await DataBaseQueries.AddInvitation(invitersUsername, invitationId, roomName);
        
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, invitationText, parseMode: ParseMode.MarkdownV2, cancellationToken: TelegramAttributes.Token);
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, 
            "Перешли сообщение выше своему другу\ud83d\ude07\n P.S. работает для одного человека, для нового друга запроси новое приглашение", cancellationToken: TelegramAttributes.Token);
    }
    
    
    //Удаление или изменение др пользователей
    private static async Task Modify(Message message)
    {
        string username = message.From.Username;
        
        State state = await DataBaseQueries.GetState(username);
        
        if (state == State.Deleting)
        {
            await DataBaseQueries.DeleteUser(username);
        }
            
        //Вывод информации о модификации
        await TelegramAttributes.BotClient.SendTextMessageAsync(message.Chat.Id, Formatter.ModifyReplies[state],
            replyMarkup:new ReplyKeyboardRemove(), cancellationToken: TelegramAttributes.Token);
    }
    
    private static async Task MaybeUpdateBirthday(Message message)
    {
        string username = message.From.Username;
        await DataBaseQueries.SetState(username, State.Updating);
        await KeyBoardReply__(message, GetYesNoKeyBoard());
    }
        
    private static async Task MaybeDeleteBd(Message message)
    {
        string username = message.From.Username;
        await DataBaseQueries.SetState(username, State.Deleting);
        await KeyBoardReply__(message, GetYesNoKeyBoard());
    }

    //TODO: провекра на update или delete Чтоб чел не написал нет и стал зареганным)
    private static async Task CancelModifications(Message message)
    {
        string username = message.From.Username;
        await DataBaseQueries.SetState(username, State.Added);
        await KeyBoardReply__(message, GetAddedUserKeyBoard());
    }

    private static ReplyKeyboardMarkup? GetAddedUserKeyBoard()
    {
        List<KeyboardButton[]> keyboards =
        [
            [
                new KeyboardButton("Привет"),
                new KeyboardButton("Пока")
            ],
            [
                new KeyboardButton("Показать все дршки")
            ],
            [
                new KeyboardButton("Отправить приглашение другу")
            ],
            [
                new KeyboardButton("Изменить моё др")
            ],
            [
                new KeyboardButton("Удалить меня из системы")
            ]
        ];

        return CreateKeyBoardMarkUp(keyboards);
    }

        
    private static ReplyKeyboardMarkup? GetYesNoKeyBoard()
    {
        List<KeyboardButton[]> keyboards =
        [
            [
                new KeyboardButton("Нет")
            ],
            [
                new KeyboardButton("Да")
            ]
        ];

        return CreateKeyBoardMarkUp(keyboards);
    }

        
    private static ReplyKeyboardMarkup? CreateKeyBoardMarkUp(List<KeyboardButton[]> keyboards)
    {
        try
        {
            return new ReplyKeyboardMarkup(keyboards) { ResizeKeyboard = true };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}