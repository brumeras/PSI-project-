using System.Collections.Concurrent;

namespace KNOTS.Services;

/// <summary>
/// Manages in-memory storage and retrieval of active <see cref="GameRoom"/> instances.
/// </summary>
/// <remarks>
/// This repository provides thread-safe operations for adding, retrieving, and removing rooms.
/// It uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> to ensure safe concurrent access
/// in a multiplayer environment.
/// </remarks>
public class RoomRepository {
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    
    /// <summary>
    /// Attempts to retrieve a room by its code.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room.</param>
    /// <param name="room">When this method returns, contains the retrieved <see cref="GameRoom"/> if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the room was found; otherwise, <c>false</c>.</returns>
    public bool TryGetRoom(string roomCode, out GameRoom? room) {
        var result = _rooms.TryGetValue(roomCode, out var foundRoom);
        room = foundRoom;
        return result;
    }
    
    
    /// <summary>
    /// Retrieves a room by its code, or <c>null</c> if not found.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room.</param>
    /// <returns>The <see cref="GameRoom"/> instance, or <c>null</c> if not found.</returns>
    public GameRoom? GetRoom(string roomCode) {
        _rooms.TryGetValue(roomCode, out var room);
        return room;
    }
    
    
    /// <summary>
    /// Adds a new room to the repository or updates an existing one with the same code.
    /// </summary>
    /// <param name="room">The <see cref="GameRoom"/> to store.</param>
    public void AddRoom(GameRoom room) { _rooms[room.RoomCode] = room; }
    
    /// <summary>
    /// Removes a room from the repository.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room to remove.</param>
    /// <returns><c>true</c> if the room was successfully removed; otherwise, <c>false</c>.</returns>
    public bool RemoveRoom(string roomCode) { return _rooms.TryRemove(roomCode, out _); }
    
    /// <summary>
    /// Retrieves all currently active rooms.
    /// </summary>
    /// <returns>A list of all <see cref="GameRoom"/> instances currently in memory.</returns>
    public List<GameRoom> GetAllRooms() { return _rooms.Values.ToList(); }
    
    /// <summary>
    /// Retrieves all active room codes.
    /// </summary>
    /// <returns>A set of all active room codes.</returns>
    public HashSet<string> GetAllRoomCodes() { return _rooms.Keys.ToHashSet(); }
    
    /// <summary>
    /// Checks whether a room with the specified code exists.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room.</param>
    /// <returns><c>true</c> if the room exists; otherwise, <c>false</c>.</returns>
    public bool RoomExists(string roomCode) { return _rooms.ContainsKey(roomCode); }
}