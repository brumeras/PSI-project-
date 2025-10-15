using System.Collections.Concurrent;

namespace KNOTS.Services;

public class PlayerMappingRepository
{
    private readonly ConcurrentDictionary<string, string> _playerToRoom = new(); // ConnectionId -> RoomCode
    private readonly ConcurrentDictionary<string, string> _connectionToUsername = new(); // ConnectionId -> Username
    public void AddPlayer(string connectionId, string username) { _connectionToUsername[connectionId] = username; }
    public void MapPlayerToRoom(string connectionId, string roomCode) { _playerToRoom[connectionId] = roomCode; }
    public string? GetPlayerUsername(string connectionId) {
        _connectionToUsername.TryGetValue(connectionId, out var username);
        return username ?? "";
    }
    public string? GetPlayerRoomCode(string connectionId) {
        _playerToRoom.TryGetValue(connectionId, out var roomCode);
        return roomCode;
    }

    public bool RemovePlayer(string connectionId, out string? roomCode, out string? username) {
        _connectionToUsername.TryGetValue(connectionId, out username);
        var hasRoom = _playerToRoom.TryRemove(connectionId, out roomCode);
        _connectionToUsername.TryRemove(connectionId, out _);
        return hasRoom;
    }
}