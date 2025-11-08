namespace KNOTS.Services;
public struct JoinRoomResult {
    public bool Success { get; set; }
    public string Message { get; set; }
    public GameState State { get; set; }
    public JoinRoomResult(bool success, string message, GameState state = GameState.WaitingForPlayers) {
        Success = success;
        Message = message;
        State = state;
    }
}
