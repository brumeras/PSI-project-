namespace KNOTS.Services;

//joingameroom funkcijos extensionas, skirtas papildyti funkcionaluma funkcijos skirtos prisijungti i kambari
//cia yra testai, kuriuos passinus galima prisijungti i kambari


/// <summary>
/// Provides extension methods for the <see cref="GameRoom"/> class to simplify
/// join validation and room state checks.
/// </summary>
/// <remarks>
/// These extensions are primarily used to extend the functionality of
/// the <c>JoinGameRoom</c> logic by providing reusable predicates for
/// checking room conditions before allowing a player to join.
/// </remarks>
public static class GameRoomExs {
    
    /// <summary>
    /// Determines whether the specified room has reached its maximum player capacity.
    /// </summary>
    /// <param name="room">The <see cref="GameRoom"/> instance to check.</param>
    /// <returns>
    /// <see langword="true"/> if the room is full; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool isFull(this GameRoom room) {return room.Players.Count >= room.MaxPlayers;}
    
    /// <summary>
    /// Determines whether the game in the specified room has already started.
    /// </summary>
    /// <param name="room">The <see cref="GameRoom"/> instance to check.</param>
    /// <returns>
    /// <see langword="true"/> if the room state is <see cref="GameState.InProgress"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool hasStarted(this GameRoom room) {return room.State == GameState.InProgress;}
    
    /// <summary>
    /// Determines whether the specified room already contains a player with the given username.
    /// </summary>
    /// <param name="room">The <see cref="GameRoom"/> instance to check.</param>
    /// <param name="username">The username to search for in the room.</param>
    /// <returns>
    /// <see langword="true"/> if a player with the given username exists; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool hasPlayer(this GameRoom room, string username) {return room.Players.Any(x => x.Username == username);}

    /// <summary>
    /// Determines whether a player can join the specified room based on its state and existing players.
    /// </summary>
    /// <param name="room">The <see cref="GameRoom"/> instance to evaluate.</param>
    /// <param name="username">The username of the player attempting to join.</param>
    /// <returns>
    /// A <see cref="JoinRoomResult"/> indicating whether the join attempt was successful and, if not,
    /// the reason for failure.
    /// </returns>
    public static JoinRoomResult CanJoin(this GameRoom room, string username) {
        if (room.isFull()) return new JoinRoomResult { Success = false, Message = "Room is full", State = GameState.InProgress};
        if (room.hasStarted()) return new JoinRoomResult { Success = false, Message = "Game has already started", State = GameState.InProgress };
        if (room.hasPlayer(username)) return new JoinRoomResult { Success = false, Message = "Username is already taken", State = room.State };
        return new JoinRoomResult { Success = true, State = room.State };
    }
    
}