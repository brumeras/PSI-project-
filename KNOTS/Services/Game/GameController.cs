using System.Collections.Generic;

namespace KNOTS.Services;

public class GameController {
    private readonly RoomRepository _roomRepository;
    public GameController(RoomRepository roomRepository) { _roomRepository = roomRepository; }
    public bool StartGame(string roomCode, List<string> statementIds) {
        var room = _roomRepository.GetRoom(roomCode);
        if (room == null) return false;
        return room.StartGame(statementIds);
    }
    public bool AreAllPlayersReady(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        if (room == null) return false;
        return room.AreAllPlayersReady();
    }
    public List<string> GetActiveStatementIds(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        return room?.ActiveStatementIds ?? new List<string>();
    }
}