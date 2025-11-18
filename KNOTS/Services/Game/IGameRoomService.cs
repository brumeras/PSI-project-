namespace KNOTS.Services;

public interface IGameRoomService {
    string CreateRoom(string hostConnectionId, string hostUsername);
    JoinRoomResult JoinRoom(string roomCode, string connectionId, string username);
    DisconnectedPlayerInfo RemovePlayer(string connectionId);
    GameRoom? GetRoomInfo(string roomCode);
    List<string> GetRoomPlayerUsernames(string roomCode);
    string GetPlayerUsername(string connectionId);
}