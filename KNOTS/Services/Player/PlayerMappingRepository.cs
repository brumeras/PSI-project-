using System.Collections.Concurrent;

namespace KNOTS.Services;
public class PlayerMappingRepository {
    private readonly ConcurrentDictionary<string, (string Username, string RoomCode)> _playerMappings = new(); 
    public void AddPlayer(string connectionId, string username, string roomCode) {_playerMappings[connectionId] = (username, roomCode); }
    public bool RemovePlayer(string connectionId, out string? roomCode, out string? username) {
        if (_playerMappings.TryRemove(connectionId, out var info)) {
            username = info.Username;
            roomCode = info.RoomCode;
            return true;
        }
        username = null;
        roomCode = null;
        return false;
    }
    public string? GetPlayerUsername(string connectionId) => _playerMappings.TryGetValue(connectionId, out var info) ? info.Username : null;
}