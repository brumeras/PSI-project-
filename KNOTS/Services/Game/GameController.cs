using System.Collections.Generic;

namespace KNOTS.Services;

/// <summary>
/// Handles high-level game control operations such as starting games
/// and checking player readiness.
/// </summary>
/// <remarks>
/// This controller interacts with the <see cref="RoomRepository"/> to manage game state
/// and ensure that all players are ready before gameplay begins.
/// </remarks>
public class GameController {
    private readonly RoomRepository _roomRepository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameController"/> class.
    /// </summary>
    /// <param name="roomRepository">Repository used to manage and access room data.</param>
    public GameController(RoomRepository roomRepository) { _roomRepository = roomRepository; }
    
    /// <summary>
    /// Starts a new game in the specified room with the given statement IDs.
    /// </summary>
    /// <param name="roomCode">Unique room code identifying the game session.</param>
    /// <param name="statementIds">List of statement IDs to include in the game.</param>
    /// <returns><c>true</c> if the game was successfully started; otherwise, <c>false</c>.</returns>
    public bool StartGame(string roomCode, List<string> statementIds) {
        var room = _roomRepository.GetRoom(roomCode);
        if (room == null) return false;
        return room.StartGame(statementIds);
    }
    
    
    /// <summary>
    /// Checks whether all players in the specified room are ready to start the game.
    /// </summary>
    /// <param name="roomCode">Unique room code identifying the game session.</param>
    /// <returns><c>true</c> if all players are ready; otherwise, <c>false</c>.</returns>
    public bool AreAllPlayersReady(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        if (room == null) return false;
        return room.AreAllPlayersReady();
    }
    
    /// <summary>
    /// Retrieves the list of currently active statement IDs for the given room.
    /// </summary>
    /// <param name="roomCode">Unique room code identifying the game session.</param>
    /// <returns>
    /// A list of active statement IDs, or an empty list if the room does not exist.
    /// </returns>
    public List<string> GetActiveStatementIds(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        return room?.ActiveStatementIds ?? new List<string>();
    }
}