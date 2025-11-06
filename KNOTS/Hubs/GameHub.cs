using Microsoft.AspNetCore.SignalR;
using KNOTS.Services;
using System.Threading.Channels;

namespace KNOTS.Hubs;
public class GameHub : Hub {
    private readonly GameRoomService _gameRoomService;
    public GameHub(GameRoomService gameRoomService) { _gameRoomService = gameRoomService;}
    public async Task CreateRoom(string username) {
            var connectionId = Context.ConnectionId;
            var roomCode = _gameRoomService.CreateRoom(connectionId, username);
            await Groups.AddToGroupAsync(connectionId, roomCode);
            await Clients.Caller.SendAsync("RoomCreated", roomCode);
    }
    public async Task JoinRoom(string roomCode, string username) {
        var connectionId = Context.ConnectionId;
        var result = _gameRoomService.JoinRoom(roomCode, connectionId, username);
            
        if (result.Success) {
            await Groups.AddToGroupAsync(connectionId, roomCode);
            var roomInfo = _gameRoomService.GetRoomInfo(roomCode);
            if (roomInfo != null){
                var roomData = new {
                    RoomCode = roomInfo.RoomCode,
                    Players = roomInfo.Players.Select(p => p.Username).ToList()
                };
                await Clients.Caller.SendAsync("JoinedRoom", roomData);
                await Clients.OthersInGroup(roomCode).SendAsync("PlayerJoinedRoom", username);
            }else {await Clients.Caller.SendAsync("JoinRoomFailed", "Room information not found"); }
        }else { await Clients.Caller.SendAsync("JoinRoomFailed", result.Message); }
    }
    public async Task SendGameAction(string roomCode, string action, object data) {
        var connectionId = Context.ConnectionId;
        var username = _gameRoomService.GetPlayerUsername(connectionId);
        await Clients.Group(roomCode).SendAsync("GameAction", username, action, data);
    }
    public async IAsyncEnumerable<object> StreamRoomUpdates(string roomCode, CancellationToken cancellationToken) {
        var channel = Channel.CreateUnbounded<object>();
        void UpdateHandler(object update) { channel.Writer.TryWrite(update); }
        try { await foreach (var update in channel.Reader.ReadAllAsync(cancellationToken)) { yield return update; } }
        finally {channel.Writer.Complete();}
    }
    public async Task UploadGameActions(IAsyncEnumerable<GameActionData> actionsStream) {
        var connectionId = Context.ConnectionId;
        var username = _gameRoomService.GetPlayerUsername(connectionId);
        await foreach (var action in actionsStream) {if (!string.IsNullOrEmpty(action.RoomCode)) {await Clients.Group(action.RoomCode).SendAsync("GameAction", username, action.Action, action.Data); }}
    }
    public async IAsyncEnumerable<PlayerStatus> StreamPlayerStatuses(
        string roomCode, 
        CancellationToken cancellationToken){
        var updateInterval = TimeSpan.FromSeconds(1);
        while (!cancellationToken.IsCancellationRequested) {
            var roomInfo = _gameRoomService.GetRoomInfo(roomCode);
                
            if (roomInfo != null) {
                foreach (var player in roomInfo.Players) {
                    yield return new PlayerStatus {
                        Username = player.Username,
                        IsOnline = true,
                        Timestamp = DateTime.UtcNow
                    };
                }
            }
            await Task.Delay(updateInterval, cancellationToken);
        }
    }
    public override async Task OnDisconnectedAsync(Exception? exception) {
        var connectionId = Context.ConnectionId;
        var disconnectedInfo = _gameRoomService.RemovePlayer(connectionId);
            
        if (!string.IsNullOrEmpty(disconnectedInfo.RoomCode)) {
            await Clients.Group(disconnectedInfo.RoomCode)
                .SendAsync("PlayerLeft", disconnectedInfo.Username);
        }
        await base.OnDisconnectedAsync(exception);
        }
    }
    public class GameActionData {
        public string RoomCode { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
    public class PlayerStatus {
        public string Username { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime Timestamp { get; set; }
    }