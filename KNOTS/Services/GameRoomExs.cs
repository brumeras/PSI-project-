namespace KNOTS.Services;

//joingameroom funkcijos extensionas, skirtas papildyti funkcionaluma funkcijos skirtos prisijungti i kambari
//cia yra testai, kuriuos passinus galima prisijungti i kambari
public static class GameRoomExs
{

    public static bool isFull(this GameRoom room) {return room.Players.Count >= room.MaxPlayers;}
    public static bool hasStarted(this GameRoom room) {return room.State == gameState.inProgress;}
    public static bool hasPlayer(this GameRoom room, string username) {return room.Players.Any(x => x.Username == username);}


    public static JoinRoomResult CanJoin(this GameRoom room, string username)
    {
        if (room.isFull())
            return new JoinRoomResult { Success = false, Message = "Room is full" };

        if (room.hasStarted())
            return new JoinRoomResult { Success = false, Message = "Game has already started" };

        if (room.hasPlayer(username))
            return new JoinRoomResult { Success = false, Message = "Username is already taken" };
        

        return new JoinRoomResult { Success = true };
    }
    
}