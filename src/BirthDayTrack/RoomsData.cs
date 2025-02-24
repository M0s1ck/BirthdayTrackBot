namespace BirthDayTrack;

public static class RoomsData
{
    internal static Room GetRoomByName(string roomName)
    {
        Dictionary<string, Room> rooms = new Dictionary<string, Room>
        {
            {
                "HseMates",
                new Room("HseMates", 700000000, 999999999)
            },
            {
                "Fam",
                new Room("Fam", 400000000, 699999999)
            },
            {
                "OldMates",
                new Room("OldMates", 100000000, 399999999)
            }
        };
        
        return rooms[roomName];
    }
    
    
}