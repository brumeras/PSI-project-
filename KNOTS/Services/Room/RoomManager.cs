namespace KNOTS.Services;

public class RoomManager
{
    private readonly RoomRepository _roomRepository;
    private readonly RoomCodeGenerator _codeGenerator;
    public RoomManager(RoomRepository roomRepository, RoomCodeGenerator codeGenerator) {
        _roomRepository = roomRepository;
        _codeGenerator = codeGenerator;
    }
    public GameRoom CreateRoom(string hostConnectionId, string hostUsername) {
        var existingCodes = _roomRepository.GetAllRoomCodes();
        var roomCode = _codeGenerator.Generate(existingCodes);

        var room = new GameRoom {
             RoomCode = roomCode,
            Host = hostUsername,
            Players = new List<GamePlayer>
            {
                new GamePlayer(hostConnectionId, hostUsername)
            }
        };
        _roomRepository.AddRoom(room);
        return room;
    }
    public void CleanupEmptyRoom(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        if (room != null && room.IsEmpty()) { _roomRepository.RemoveRoom(roomCode); }
    }
    public void TransferHostIfNeeded(GameRoom room, string disconnectedUsername) {
        if (room.Host == disconnectedUsername && !room.IsEmpty()) { room.TransferHost(); }
    }
}