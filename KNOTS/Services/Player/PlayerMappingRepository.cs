using System.Collections.Concurrent;

namespace KNOTS.Services;


/// <summary>
/// Provides a thread-safe in-memory mapping between players, their connection IDs,
/// usernames, and associated game rooms.
/// </summary>
/// <remarks>
/// This repository is used by various services (e.g., <see cref="PlayerManager"/> and
/// <see cref="RoomQueryService"/>) to quickly retrieve or update relationships between
/// players and rooms during gameplay.
/// </remarks>
public class PlayerMappingRepository
{
    private readonly ConcurrentDictionary<string, string> _playerToRoom = new(); // ConnectionId -> RoomCode
    private readonly ConcurrentDictionary<string, string> _connectionToUsername = new(); // ConnectionId -> Username
    
    
    /// <summary>
    /// Associates a player's connection ID with their username.
    /// </summary>
    /// <param name="connectionId">The unique connection identifier of the player.</param>
    /// <param name="username">The username of the player.</param>
    public void AddPlayer(string connectionId, string username) { _connectionToUsername[connectionId] = username; }
    
    /// <summary>
    /// Maps a player's connection ID to the room they are currently in.
    /// </summary>
    /// <param name="connectionId">The unique connection identifier of the player.</param>
    /// <param name="roomCode">The code of the room the player has joined.</param>
    public void MapPlayerToRoom(string connectionId, string roomCode) { _playerToRoom[connectionId] = roomCode; }
    
    
    /// <summary>
    /// Retrieves the username associated with the given connection ID.
    /// </summary>
    /// <param name="connectionId">The unique connection identifier of the player.</param>
    /// <returns>The player's username if found; otherwise, an empty string.</returns>
    public string? GetPlayerUsername(string connectionId) {
        _connectionToUsername.TryGetValue(connectionId, out var username);
        return username ?? "";
    }
    
    /// <summary>
    /// Retrieves the room code associated with the given connection ID.
    /// </summary>
    /// <param name="connectionId">The unique connection identifier of the player.</param>
    /// <returns>The room code if the player is in a room; otherwise, <see langword="null"/>.</returns>
    public string? GetPlayerRoomCode(string connectionId) {
        _playerToRoom.TryGetValue(connectionId, out var roomCode);
        return roomCode;
    }

    
    /// <summary>
    /// Removes a player's mappings from both username and room dictionaries.
    /// </summary>
    /// <param name="connectionId">The unique connection identifier of the player to remove.</param>
    /// <param name="roomCode">Outputs the room code the player was associated with, if any.</param>
    /// <param name="username">Outputs the username of the player being removed, if any.</param>
    /// <returns><see langword="true"/> if the player was mapped to a room; otherwise, <see langword="false"/>.</returns>
    public bool RemovePlayer(string connectionId, out string? roomCode, out string? username) {
        _connectionToUsername.TryGetValue(connectionId, out username);
        var hasRoom = _playerToRoom.TryRemove(connectionId, out roomCode);
        _connectionToUsername.TryRemove(connectionId, out _);
        return hasRoom;
    }
}