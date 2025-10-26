using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;

public class RoomQueryService {
    private readonly RoomRepository _roomRepository;
    private readonly PlayerMappingRepository _playerMappingRepository;
    public RoomQueryService(RoomRepository roomRepository, PlayerMappingRepository playerMappingRepository) {
        _roomRepository = roomRepository;
        _playerMappingRepository = playerMappingRepository;
    }
    public GameRoom? GetRoomInfo(string roomCode) { return _roomRepository.GetRoom(roomCode); }
    public List<string> GetRoomPlayerUsernames(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        return room?.Players.Select(p => p.Username).ToList() ?? new List<string>();
    }
    public string GetPlayerUsername(string connectionId) { return _playerMappingRepository.GetPlayerUsername(connectionId) ?? ""; }
    public string? GetPlayerRoomCode(string connectionId) { return _playerMappingRepository.GetPlayerRoomCode(connectionId); }
    public bool IsPlayerHost(string connectionId) {
        var roomCode = _playerMappingRepository.GetPlayerRoomCode(connectionId);
        if (roomCode == null) return false;

        var room = _roomRepository.GetRoom(roomCode);
        if (room == null) return false;

        var username = _playerMappingRepository.GetPlayerUsername(connectionId);
        return room.Host == username;
    }
    public List<GameRoom> GetAllRooms() { return _roomRepository.GetAllRooms(); }
}