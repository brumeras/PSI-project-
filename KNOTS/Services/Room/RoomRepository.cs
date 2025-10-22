using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;

public class RoomRepository {
    private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    public bool TryGetRoom(string roomCode, out GameRoom? room) {
        var result = _rooms.TryGetValue(roomCode, out var foundRoom);
        room = foundRoom;
        return result;
    }
    public GameRoom? GetRoom(string roomCode) {
        _rooms.TryGetValue(roomCode, out var room);
        return room;
    }
    public void AddRoom(GameRoom room) { _rooms[room.RoomCode] = room; }
    public bool RemoveRoom(string roomCode) { return _rooms.TryRemove(roomCode, out _); }
    public List<GameRoom> GetAllRooms() { return _rooms.Values.ToList(); }
    public HashSet<string> GetAllRoomCodes() { return _rooms.Keys.ToHashSet(); }
    
    public bool RoomExists(string roomCode) { return _rooms.ContainsKey(roomCode); }
}