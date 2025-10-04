namespace KNOTS.Services;

//joingameroom funkcijos extensionas, skirtas papildyti funkcionaluma funkcijos skirtos prisijungti i kambari
//cia yra testai, kuriuos passinus galima prisijungti i kambari
public static class GameRoomExs
{
    public static JoinRoomResult CanJoin(this GameRoom room, string username)
    {
        if (room.Players.Count >= room.MaxPlayers)
        {
            return new JoinRoomResult 
            { 
                Success = false, 
                Message = "Room is full" 
            };
        }

        if (room.IsGameStarted)
        {
            return new JoinRoomResult
            {
                Success = false,
                Message = "Game has already started"
            };
        }

        if (room.Players.Any(x => x.Username == username))
        {
            return new JoinRoomResult
            {
                Success = false,
                Message = "Username is already taken"
            };
        }

        return new JoinRoomResult { Success = true };
    }
    
}