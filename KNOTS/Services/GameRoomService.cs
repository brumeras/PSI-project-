using System.Collections.Concurrent;

namespace KNOTS.Services
{
    public enum GameState
    {
        waitingForPlayers,
        inProgress,
        finished
    }
    public class GameRoom
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public List<GamePlayer> Players { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        //public bool IsGameStarted { get; set; } = false;
        public GameState State { get; set; } = GameState.waitingForPlayers;
        public int MaxPlayers { get; set; } = 4;
    }

    public class GamePlayer
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; } = DateTime.Now;
        public bool IsReady { get; set; } = false;
    }

    public class JoinRoomResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GameState State { get; set; }
    }

    public class DisconnectedPlayerInfo
    {
        public string Username { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
    }

    public class GameRoomService
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
        private readonly ConcurrentDictionary<string, string> _playerToRoom = new(); // ConnectionId -> RoomCode
        private readonly ConcurrentDictionary<string, string> _connectionToUsername = new(); // ConnectionId -> Username
        private readonly Random _random = new();

        // Generuoja unikalų kambario kodą
        private string GenerateRoomCode()
        {
            string code;
            do { code = _random.Next(1000, 9999).ToString(); } while (_rooms.ContainsKey(code)); return code;
        }
        // Prideda žaidėją prie sistemos
        public void AddPlayer(string connectionId, string username) { _connectionToUsername[connectionId] = username; }

        // Sukuria naują kambarį
        public string CreateRoom(string hostConnectionId, string hostUsername) {
            var roomCode = GenerateRoomCode();
            var room = new GameRoom
            {
                RoomCode = roomCode,
                Host = hostUsername,
                Players = new List<GamePlayer>
                {
                    new GamePlayer
                    {
                        ConnectionId = hostConnectionId,
                        Username = hostUsername,
                        JoinedAt = DateTime.Now
                    }
                }
            };

            _rooms[roomCode] = room;
            _playerToRoom[hostConnectionId] = roomCode;

            return roomCode;
        }

        // Prisijungia prie kambario
        public JoinRoomResult JoinRoom(string roomCode, string connectionId, string username) {
            if (!_rooms.TryGetValue(roomCode, out var room)) return new JoinRoomResult { Success = false, Message = "Room not found", State = GameState.finished };

            var result = room.CanJoin(username);
            if (!result.Success) return result;
            
            var player = new GamePlayer
            {
                ConnectionId = connectionId,
                Username = username,
                JoinedAt = DateTime.Now
            };

            room.Players.Add(player);
            _playerToRoom[connectionId] = roomCode;
            _connectionToUsername[connectionId] = username;
            
            if (room.Players.Count >= room.MaxPlayers) { room.State = GameState.inProgress; }

            return new JoinRoomResult 
            { 
                Success = true, 
                Message = "Successfully connected to a room!",
                State = room.State
            };
        }

        // Gauna kambario informaciją
        public GameRoom? GetRoomInfo(string roomCode) {
            _rooms.TryGetValue(roomCode, out var room);
            return room;
        }

        // Gauna žaidėjo vardą pagal ConnectionId
        public string GetPlayerUsername(string connectionId) {
            _connectionToUsername.TryGetValue(connectionId, out var username);
            return username ?? "";
        }

        // Pašalina žaidėją iš sistemos
        public DisconnectedPlayerInfo RemovePlayer(string connectionId) {
            var result = new DisconnectedPlayerInfo();
            
            _connectionToUsername.TryGetValue(connectionId, out var username);
            result.Username = username ?? "";

            if (_playerToRoom.TryRemove(connectionId, out var roomCode)) {
                result.RoomCode = roomCode;
                
                if (_rooms.TryGetValue(roomCode, out var room)) { 
                    room.Players.RemoveAll(p => p.ConnectionId == connectionId);
                    // Jei kambarys tuščias, pašalinam jį
                    if (room.Players.Count == 0) { _rooms.TryRemove(roomCode, out _); }
                    // Jei išėjo host'as, paskiriame naują
                    else if (room.Host == username && room.Players.Count > 0) { room.Host = room.Players.First().Username; }
                }
            }
            _connectionToUsername.TryRemove(connectionId, out _);
            return result;
        }

        // Gauna visų kambarių sąrašą (debug tikslams)
        public List<GameRoom> GetAllRooms() { return _rooms.Values.ToList(); }
        // Gauna kambario kodą pagal žaidėjo ConnectionId
        public string? GetPlayerRoomCode(string connectionId) {
            _playerToRoom.TryGetValue(connectionId, out var roomCode);
            return roomCode;
        }
    }
}