using System.Data.Common;
using AccessConfiguration;
using MySql.Data.MySqlClient;

namespace BirthDayTrack;

/// <summary>
/// Класс для работы с бд.
/// Тут реализован CRUD для базы данных с пользователями, а также получение id чатов и именниников для уведомлений о дршках
/// </summary>
public static class DataBaseQueries
{
    internal static string ConnectionString;
    private static async Task<MySqlConnection> Connect()
    {
        MySqlConnection connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();  // Соединение открыто
        return connection;
    }
    
    internal static async Task<IOrderedEnumerable<Tuple<string, DateOnly>>> GetAllUsers()
    {
        string query = "SELECT * FROM tgusers";

        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        await using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
        
        List<Tuple<string, DateOnly>> usersBdays = [];

        while (await reader.ReadAsync())
        {
            string username = reader.GetString("username");
            DateTime birthdayDt = reader.GetDateTime("birthday");
            DateOnly birthday = DateOnly.FromDateTime(birthdayDt);
            Tuple<string, DateOnly> user = new(username, birthday);
            usersBdays.Add(user);
        }
        
        //Сортировка по ближайшим дршкам
        IOrderedEnumerable<Tuple<string, DateOnly>> orderedUsersQuery = usersBdays.OrderBy(ubd => 
            ubd.Item2.DayOfYear >= DateTime.Today.DayOfYear ? ubd.Item2.DayOfYear - DateTime.Today.DayOfYear: 365 + ubd.Item2.DayOfYear - DateTime.Today.DayOfYear);
        
        return orderedUsersQuery;
    }

    
    internal static async Task AddUser(string username, long chatId, string date)
    {
        const string query = "INSERT INTO tgusers (username, birthday, chatId) VALUES (@username, @birthday, @chatId)";
    
        await using MySqlConnection connection =  await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        
        string[] monday = date.Split(['.', ' ']);
        DateTime birthday = new DateTime(2006, int.Parse(monday[1]), int.Parse(monday[0]));
    
        command.Parameters.AddWithValue("@username", $"@{username}");
        command.Parameters.AddWithValue("@birthday", birthday.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@chatId", chatId);
        await command.ExecuteNonQueryAsync();
    }

    
    internal static async Task SetBirthday(string username, string date)
    {
        string[] monday = date.Split(['.', ' ']);
        string month = monday[1];
        string day = monday[0];
        
        string fullDate = $"2006-{month}-{day}";

        const string query = "UPDATE tgusers SET birthday = @birthday, state = @newState WHERE username = @username;";
        
        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);

        command.Parameters.AddWithValue("@birthday", fullDate);
        command.Parameters.AddWithValue("@username", $"@{username}");
        command.Parameters.AddWithValue("@newState", "Added");
        
        await command.ExecuteNonQueryAsync();
    }

    
    internal static async Task DeleteUser(string username)
    {
        string query = "DELETE FROM tgusers WHERE username = @username";
    
        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
    
        command.Parameters.AddWithValue("@username", username.StartsWith("@") ? username : $"@{username}");
        await command.ExecuteNonQueryAsync();
    }

    
    internal static async Task<List<long>> GetAllChatIds()
    {
        string query = "SELECT chatId FROM tgusers";

        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        await using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

        List<long> chatIds = [];

        while (await reader.ReadAsync())
        {
            long chatId = reader.GetInt64("chatId");
            chatIds.Add(chatId);
        }

        return chatIds;
    }


    internal static async Task<List<string>> GetBirthDayBoys()
    {
        string query = "SELECT username, birthday  FROM tgusers";

        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        await using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

        List<string> birthdayBoys = [];

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        Console.WriteLine($"today: {today.ToString()}");

        while (await reader.ReadAsync())
        {
            DateTime birthdayDateTime = reader.GetDateTime("birthday");
            DateOnly birthday = DateOnly.FromDateTime(birthdayDateTime);
            
            if (birthday.DayOfYear == today.DayOfYear)
            {
                string username = reader.GetString("username");
                birthdayBoys.Add(username);
            }
        }

        return birthdayBoys;
    }

    
    internal static async Task<string> GetUsersRoom(string username)
    {
        const string query = "SELECT room FROM tgusers WHERE username = @username LIMIT 1";

        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);

        command.Parameters.AddWithValue("@username", $"@{username}");
        
        MySqlDataReader reader = command.ExecuteReader();

        reader.Read();
        
        string room = reader.GetString("room");
        return room;
    }

    
    internal static async Task AddInvitation(string username, int code, string roomName)
    {
        const string query = "INSERT INTO invitationcodes (code, inviter, room) VALUES (@code, @inviter, @room)";
        
        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@code", code);
        command.Parameters.AddWithValue("@inviter", $"@{username}");
        command.Parameters.AddWithValue("@room", roomName);

        await command.ExecuteNonQueryAsync();
    }

    internal static async Task<string?> GetRoomByInvCode(int invCode)
    {
        const string query = "SELECT id, code, room FROM invitationcodes WHERE code = @code";
        
        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@code", invCode);
        
        await using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
        
        string? res = reader.Read() ? reader.GetString("room") : null;
        await reader.CloseAsync();
        
        //Удаление использованного кода на месте
        const string deleteQuery = "DELETE FROM invitationcodes WHERE code = @code";
        
        await using MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);
        deleteCommand.Parameters.AddWithValue("@code", invCode);

        await deleteCommand.ExecuteNonQueryAsync();

        return res;
    }
    
    
    internal static async Task AddInvitedUser(string username, long chatId, string roomName)
    {
        const string query =
            "INSERT INTO tgusers (username, birthday, chatId, room, state) VALUES (@username, @birthday, @chatId, @room, @state)";
        
        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@username", $"@{username}");
        command.Parameters.AddWithValue("@birthday", null);
        command.Parameters.AddWithValue("@chatId", chatId);
        command.Parameters.AddWithValue("@room", roomName);
        command.Parameters.AddWithValue("@state", "EnteringBirthday");

        await command.ExecuteNonQueryAsync();
    }

    internal static async Task<State> GetState(string username)
    {
        const string query = "SELECT state FROM tgusers WHERE username = @username LIMIT 1";
        
        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);

        command.Parameters.AddWithValue("@username", $"@{username}");
        
        MySqlDataReader reader = command.ExecuteReader();

        reader.Read();
        
        string strState = reader.GetString("state");

        State state = Enum.Parse<State>(strState);
        return state;
    }
    
    internal static async Task SetState(string username, State newtState)
    {
        const string query = "UPDATE tgusers SET state = @newState WHERE username = @username";
        
        await using MySqlConnection connection = await Connect();
        await using MySqlCommand command = new MySqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@username", $"@{username}");
        command.Parameters.AddWithValue("@newState", newtState.ToString());

        await command.ExecuteNonQueryAsync();
    }

}