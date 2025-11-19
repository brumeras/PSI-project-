using KNOTS.Services;

namespace TestProject1.GameRoomExs_Tests;

public class GameRoomBase {
    public GameRoom NewRoom(int max = 4, GameState state = GameState.WaitingForPlayers) => new() {
        MaxPlayers = max,
        State = state,
        Players = new(),
        Host = ""
    };
    public GamePlayer Player(string id, string name) => new(id, name);
}