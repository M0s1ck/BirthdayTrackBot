using Microsoft.VisualBasic;

namespace BirthDayTrack;

internal class Room
{
    internal string RoomName;
    internal int MaxId;
    internal int MinId;

    private static readonly Random Random = new Random();

    internal Room(string roomName, int minId, int maxId)
    {
        RoomName = roomName;
        MinId = minId;
        MaxId = maxId;
    }

    internal int GetInvitationId()
    {
        return Random.Next(MinId, MaxId + 1);
    }

    internal string GetInvitationMessage(int id)
    {
        return $"Привет\\-привет\\! \ud83d\ude0f\n Это бот для напоминаний о дршках твоих друзей: @BirthDayTrackBot\\.\n Ваш код приглашения: `{id}`";
    }
}