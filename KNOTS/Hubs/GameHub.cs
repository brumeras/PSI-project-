using Microsoft.AspNetCore.SignalR;
using KNOTS.Services;
using System.Threading.Channels;

namespace KNOTS.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameRoomService _gameRoomService;
        
        public GameHub(GameRoomService gameRoomService)
        {
            _gameRoomService = gameRoomService;
        }

        // Vartotojas prisijungia prie žaidimo
        public async Task JoinGame(string username)
        {
            var connectionId = Context.ConnectionId;
            _gameRoomService.AddPlayer(connectionId, username);
            
            // Pranešame visiem apie naują žaidėją
            await Clients.All.SendAsync("PlayerJoined", username);
            
            // Siunčiame žaidėjui jo unikalų ID
            await Clients.Caller.SendAsync("AssignPlayerId", connectionId);
        }

        // Sukurti naują kambarį
        public async Task CreateRoom(string username)
        {
            var connectionId = Context.ConnectionId;
            var roomCode = _gameRoomService.CreateRoom(connectionId, username);
            
            // Prisijungiam prie šio kambario grupės (BE "Room_" prefix!)
            await Groups.AddToGroupAsync(connectionId, roomCode);
            
            // Pranešame žaidėjui apie sėkmingą kambario sukūrimą
            await Clients.Caller.SendAsync("RoomCreated", roomCode);
        }

        // Prisijungti prie kambario
        public async Task JoinRoom(string roomCode, string username)
        {
            var connectionId = Context.ConnectionId;
            var result = _gameRoomService.JoinRoom(roomCode, connectionId, username);
            
            if (result.Success)
            {
                // Prisijungiam prie kambario grupės (BE "Room_" prefix!)
                await Groups.AddToGroupAsync(connectionId, roomCode);
                
                // Gauname kambario informaciją
                var roomInfo = _gameRoomService.GetRoomInfo(roomCode);
                
                if (roomInfo != null)
                {
                    // Siunčiam žaidėjui supaprastintą kambario informaciją
                    var roomData = new
                    {
                        RoomCode = roomInfo.RoomCode,
                        Players = roomInfo.Players.Select(p => p.Username).ToList()
                    };
                    
                    await Clients.Caller.SendAsync("JoinedRoom", roomData);
                    
                    // Pranešame KITIEMS kambaryje apie naują žaidėją (ne caller'iui)
                    await Clients.OthersInGroup(roomCode).SendAsync("PlayerJoinedRoom", username);
                }
                else
                {
                    await Clients.Caller.SendAsync("JoinRoomFailed", "Room information not found");
                }
            }
            else
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", result.Message);
            }
        }

        // Išsiųsti žaidimo ėjimą/veiksmą
        public async Task SendGameAction(string roomCode, string action, object data)
        {
            var connectionId = Context.ConnectionId;
            var username = _gameRoomService.GetPlayerUsername(connectionId);
            
            // Persiųsti veiksmą visiems kambaryje esantiems žaidėjams
            await Clients.Group(roomCode).SendAsync("GameAction", username, action, data);
        }

        // Streaming metodas - gauti kambario atnaujinimus realiu laiku
        public async IAsyncEnumerable<object> StreamRoomUpdates(string roomCode, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<object>();
            
            // Registruojame listener'į kambario atnaujinimams
            void UpdateHandler(object update)
            {
                channel.Writer.TryWrite(update);
            }
            
            try
            {
                await foreach (var update in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    yield return update;
                }
            }
            finally
            {
                // Išvalome listener'į
                // _gameRoomService.OnRoomUpdate -= UpdateHandler;
                channel.Writer.Complete();
            }
        }

        // Streaming metodas - siųsti žaidimo veiksmus kaip srautą
        public async Task UploadGameActions(IAsyncEnumerable<GameActionData> actionsStream)
        {
            var connectionId = Context.ConnectionId;
            var username = _gameRoomService.GetPlayerUsername(connectionId);
            
            await foreach (var action in actionsStream)
            {
                // Apdorojame kiekvieną veiksmą iš srauto
                if (!string.IsNullOrEmpty(action.RoomCode))
                {
                    await Clients.Group(action.RoomCode)
                        .SendAsync("GameAction", username, action.Action, action.Data);
                }
            }
        }

        // Streaming metodas - gauti žaidėjų būsenas realiu laiku
        public async IAsyncEnumerable<PlayerStatus> StreamPlayerStatuses(
            string roomCode, 
            CancellationToken cancellationToken)
        {
            var updateInterval = TimeSpan.FromSeconds(1);
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var roomInfo = _gameRoomService.GetRoomInfo(roomCode);
                
                if (roomInfo != null)
                {
                    foreach (var player in roomInfo.Players)
                    {
                        yield return new PlayerStatus
                        {
                            Username = player.Username,
                            IsOnline = true,
                            Timestamp = DateTime.UtcNow
                        };
                    }
                }
                
                await Task.Delay(updateInterval, cancellationToken);
            }
        }

        // Atsijungimo valdymas
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var disconnectedInfo = _gameRoomService.RemovePlayer(connectionId);
            
            if (!string.IsNullOrEmpty(disconnectedInfo.RoomCode))
            {
                await Clients.Group(disconnectedInfo.RoomCode)
                    .SendAsync("PlayerLeft", disconnectedInfo.Username);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }

    // Gettreriai ir setteriai streamingo funkcionalumui
    public class GameActionData
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class PlayerStatus
    {
        public string Username { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime Timestamp { get; set; }
    }
}