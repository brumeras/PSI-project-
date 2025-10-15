using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services
{
    
    public enum GameState
    {
        WaitingForPlayers,
        InProgress,
        Finished
    }

    public struct GamePlayer
    {
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsReady { get; set; }
        
        public GamePlayer(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;
            JoinedAt = DateTime.Now;
            IsReady = false;
        }
    }

    public struct JoinRoomResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public GameState State { get; set; }
        
        public JoinRoomResult(bool success, string message, GameState state = GameState.WaitingForPlayers)
        {
            Success = success;
            Message = message;
            State = state;
        }
    }

    public struct DisconnectedPlayerInfo
    {
        public string Username { get; set; }
        public string RoomCode { get; set; }
        
        public DisconnectedPlayerInfo(string username, string roomCode)
        {
            Username = username;
            RoomCode = roomCode;
        }
    }

    // ===== GAME ROOM ENTITY =====
    
    public class GameRoom
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public List<GamePlayer> Players { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public GameState State { get; set; } = GameState.WaitingForPlayers;
        public int MaxPlayers { get; set; } = 4;
        public List<string> ActiveStatementIds { get; set; } = new();

        // Domain logic methods 
        public JoinRoomResult CanJoin(string username)
        {
            if (State != GameState.WaitingForPlayers)
                return new JoinRoomResult(false, "Game already in progress", State);

            if (Players.Count >= MaxPlayers)
                return new JoinRoomResult(false, "Room is full", State);

            if (Players.Any(p => p.Username == username))
                return new JoinRoomResult(false, "Username already taken in this room", State);

            return new JoinRoomResult(true, "Can join", State);
        }

        public void AddPlayer(GamePlayer player)
        {
            Players.Add(player);
            
            if (Players.Count >= MaxPlayers)
                State = GameState.InProgress;
        }

        public bool RemovePlayer(string connectionId)
        {
            var removed = Players.RemoveAll(p => p.ConnectionId == connectionId) > 0;
            return removed;
        }

        public bool SetPlayerReady(string connectionId, bool isReady)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].ConnectionId == connectionId)
                {
                    var player = Players[i];
                    player.IsReady = isReady;
                    Players[i] = player;
                    return true;
                }
            }
            return false;
        }

        public bool AreAllPlayersReady()
        {
            if (Players.Count < 2)
                return false;

            return Players.All(p => p.IsReady);
        }

        public bool StartGame(List<string> statementIds)
        {
            if (State == GameState.InProgress)
                return false;

            State = GameState.InProgress;
            ActiveStatementIds = statementIds;
            return true;
        }

        public void TransferHost()
        {
            if (Players.Any())
                Host = Players.First().Username;
        }

        public bool IsEmpty() => Players.Count == 0;
    }

    // ===== ROOM CODE GENERATOR =====
    
    public class RoomCodeGenerator
    {
        private readonly Random _random = new();

        public string Generate(HashSet<string> existingCodes)
        {
            string code;
            do
            {
                code = _random.Next(1000, 9999).ToString();
            }
            while (existingCodes.Contains(code));

            return code;
        }
    }

    // ===== ROOM REPOSITORY  =====
    
    public class RoomRepository
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();

        public bool TryGetRoom(string roomCode, out GameRoom? room)
        {
            var result = _rooms.TryGetValue(roomCode, out var foundRoom);
            room = foundRoom;
            return result;
        }

        public GameRoom? GetRoom(string roomCode)
        {
            _rooms.TryGetValue(roomCode, out var room);
            return room;
        }

        public void AddRoom(GameRoom room)
        {
            _rooms[room.RoomCode] = room;
        }

        public bool RemoveRoom(string roomCode)
        {
            return _rooms.TryRemove(roomCode, out _);
        }

        public List<GameRoom> GetAllRooms()
        {
            return _rooms.Values.ToList();
        }

        public HashSet<string> GetAllRoomCodes()
        {
            return _rooms.Keys.ToHashSet();
        }

        public bool RoomExists(string roomCode)
        {
            return _rooms.ContainsKey(roomCode);
        }
    }

    // ===== PLAYER MAPPING REPOSITORY  =====
    
    public class PlayerMappingRepository
    {
        private readonly ConcurrentDictionary<string, string> _playerToRoom = new(); // ConnectionId -> RoomCode
        private readonly ConcurrentDictionary<string, string> _connectionToUsername = new(); // ConnectionId -> Username

        public void AddPlayer(string connectionId, string username)
        {
            _connectionToUsername[connectionId] = username;
        }

        public void MapPlayerToRoom(string connectionId, string roomCode)
        {
            _playerToRoom[connectionId] = roomCode;
        }

        public string? GetPlayerUsername(string connectionId)
        {
            _connectionToUsername.TryGetValue(connectionId, out var username);
            return username ?? "";
        }

        public string? GetPlayerRoomCode(string connectionId)
        {
            _playerToRoom.TryGetValue(connectionId, out var roomCode);
            return roomCode;
        }

        public bool RemovePlayer(string connectionId, out string? roomCode, out string? username)
        {
            _connectionToUsername.TryGetValue(connectionId, out username);
            var hasRoom = _playerToRoom.TryRemove(connectionId, out roomCode);
            _connectionToUsername.TryRemove(connectionId, out _);
            return hasRoom;
        }
    }

    // ===== ROOM MANAGER =====
    
    public class RoomManager
    {
        private readonly RoomRepository _roomRepository;
        private readonly RoomCodeGenerator _codeGenerator;

        public RoomManager(RoomRepository roomRepository, RoomCodeGenerator codeGenerator)
        {
            _roomRepository = roomRepository;
            _codeGenerator = codeGenerator;
        }

        public GameRoom CreateRoom(string hostConnectionId, string hostUsername)
        {
            var existingCodes = _roomRepository.GetAllRoomCodes();
            var roomCode = _codeGenerator.Generate(existingCodes);

            var room = new GameRoom
            {
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

        public void CleanupEmptyRoom(string roomCode)
        {
            var room = _roomRepository.GetRoom(roomCode);
            if (room != null && room.IsEmpty())
            {
                _roomRepository.RemoveRoom(roomCode);
            }
        }

        public void TransferHostIfNeeded(GameRoom room, string disconnectedUsername)
        {
            if (room.Host == disconnectedUsername && !room.IsEmpty())
            {
                room.TransferHost();
            }
        }
    }

    // ===== PLAYER MANAGER  =====
    
    public class PlayerManager
    {
        private readonly RoomRepository _roomRepository;
        private readonly PlayerMappingRepository _playerMappingRepository;
        private readonly RoomManager _roomManager;

        public PlayerManager(
            RoomRepository roomRepository,
            PlayerMappingRepository playerMappingRepository,
            RoomManager roomManager)
        {
            _roomRepository = roomRepository;
            _playerMappingRepository = playerMappingRepository;
            _roomManager = roomManager;
        }

        public JoinRoomResult JoinRoom(string roomCode, string connectionId, string username)
        {
            if (!_roomRepository.TryGetRoom(roomCode, out var room) || room == null)
                return new JoinRoomResult(false, "Room not found", GameState.Finished);

            var canJoinResult = room.CanJoin(username);
            if (!canJoinResult.Success)
                return canJoinResult;

            var player = new GamePlayer(connectionId, username);
            room.AddPlayer(player);

            _playerMappingRepository.MapPlayerToRoom(connectionId, roomCode);
            _playerMappingRepository.AddPlayer(connectionId, username);

            return new JoinRoomResult(true, "Successfully connected to a room!", room.State);
        }

        public DisconnectedPlayerInfo RemovePlayer(string connectionId)
        {
            var hasRoom = _playerMappingRepository.RemovePlayer(connectionId, out var roomCode, out var username);
            
            var disconnectedUsername = username ?? "";
            var disconnectedRoomCode = "";

            if (hasRoom && roomCode != null)
            {
                disconnectedRoomCode = roomCode;
                var room = _roomRepository.GetRoom(roomCode);

                if (room != null)
                {
                    room.RemovePlayer(connectionId);
                    _roomManager.TransferHostIfNeeded(room, disconnectedUsername);
                    _roomManager.CleanupEmptyRoom(roomCode);
                }
            }

            return new DisconnectedPlayerInfo(disconnectedUsername, disconnectedRoomCode);
        }

        public bool SetPlayerReady(string connectionId, bool isReady)
        {
            var roomCode = _playerMappingRepository.GetPlayerRoomCode(connectionId);
            if (roomCode == null)
                return false;

            var room = _roomRepository.GetRoom(roomCode);
            if (room == null)
                return false;

            return room.SetPlayerReady(connectionId, isReady);
        }
    }

    // ===== GAME CONTROLLER =====
    
    public class GameController
    {
        private readonly RoomRepository _roomRepository;

        public GameController(RoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public bool StartGame(string roomCode, List<string> statementIds)
        {
            var room = _roomRepository.GetRoom(roomCode);
            if (room == null)
                return false;

            return room.StartGame(statementIds);
        }

        public bool AreAllPlayersReady(string roomCode)
        {
            var room = _roomRepository.GetRoom(roomCode);
            if (room == null)
                return false;

            return room.AreAllPlayersReady();
        }

        public List<string> GetActiveStatementIds(string roomCode)
        {
            var room = _roomRepository.GetRoom(roomCode);
            return room?.ActiveStatementIds ?? new List<string>();
        }
    }

    // ===== QUERY SERVICE =====
    
    public class RoomQueryService
    {
        private readonly RoomRepository _roomRepository;
        private readonly PlayerMappingRepository _playerMappingRepository;

        public RoomQueryService(RoomRepository roomRepository, PlayerMappingRepository playerMappingRepository)
        {
            _roomRepository = roomRepository;
            _playerMappingRepository = playerMappingRepository;
        }

        public GameRoom? GetRoomInfo(string roomCode)
        {
            return _roomRepository.GetRoom(roomCode);
        }

        public List<string> GetRoomPlayerUsernames(string roomCode)
        {
            var room = _roomRepository.GetRoom(roomCode);
            return room?.Players.Select(p => p.Username).ToList() ?? new List<string>();
        }

        public string GetPlayerUsername(string connectionId)
        {
            return _playerMappingRepository.GetPlayerUsername(connectionId) ?? "";
        }

        public string? GetPlayerRoomCode(string connectionId)
        {
            return _playerMappingRepository.GetPlayerRoomCode(connectionId);
        }

        public bool IsPlayerHost(string connectionId)
        {
            var roomCode = _playerMappingRepository.GetPlayerRoomCode(connectionId);
            if (roomCode == null)
                return false;

            var room = _roomRepository.GetRoom(roomCode);
            if (room == null)
                return false;

            var username = _playerMappingRepository.GetPlayerUsername(connectionId);
            return room.Host == username;
        }

        public List<GameRoom> GetAllRooms()
        {
            return _roomRepository.GetAllRooms();
        }
    }

    // ===== MAIN SERVICE =====
    
    public class GameRoomService
    {
        private readonly RoomRepository _roomRepository;
        private readonly PlayerMappingRepository _playerMappingRepository;
        private readonly RoomManager _roomManager;
        private readonly PlayerManager _playerManager;
        private readonly GameController _gameController;
        private readonly RoomQueryService _queryService;

        public GameRoomService()
        {
            _roomRepository = new RoomRepository();
            _playerMappingRepository = new PlayerMappingRepository();
            
            var codeGenerator = new RoomCodeGenerator();
            _roomManager = new RoomManager(_roomRepository, codeGenerator);
            _playerManager = new PlayerManager(_roomRepository, _playerMappingRepository, _roomManager);
            _gameController = new GameController(_roomRepository);
            _queryService = new RoomQueryService(_roomRepository, _playerMappingRepository);
        }

        // Player registration
        public void AddPlayer(string connectionId, string username)
        {
            _playerMappingRepository.AddPlayer(connectionId, username);
        }

        // Room creation
        public string CreateRoom(string hostConnectionId, string hostUsername)
        {
            var room = _roomManager.CreateRoom(hostConnectionId, hostUsername);
            _playerMappingRepository.MapPlayerToRoom(hostConnectionId, room.RoomCode);
            return room.RoomCode;
        }

        // Room joining
        public JoinRoomResult JoinRoom(string roomCode, string connectionId, string username)
        {
            return _playerManager.JoinRoom(roomCode, connectionId, username);
        }

        // Player management
        public DisconnectedPlayerInfo RemovePlayer(string connectionId)
        {
            return _playerManager.RemovePlayer(connectionId);
        }

        public bool SetPlayerReady(string connectionId, bool isReady)
        {
            return _playerManager.SetPlayerReady(connectionId, isReady);
        }

        // Game flow
        public bool StartGame(string roomCode, List<string> statementIds)
        {
            return _gameController.StartGame(roomCode, statementIds);
        }

        public bool AreAllPlayersReady(string roomCode)
        {
            return _gameController.AreAllPlayersReady(roomCode);
        }

        public List<string> GetActiveStatementIds(string roomCode)
        {
            return _gameController.GetActiveStatementIds(roomCode);
        }

        // Query operations
        public GameRoom? GetRoomInfo(string roomCode)
        {
            return _queryService.GetRoomInfo(roomCode);
        }

        public List<string> GetRoomPlayerUsernames(string roomCode)
        {
            return _queryService.GetRoomPlayerUsernames(roomCode);
        }

        public string GetPlayerUsername(string connectionId)
        {
            return _queryService.GetPlayerUsername(connectionId);
        }

        public string? GetPlayerRoomCode(string connectionId)
        {
            return _queryService.GetPlayerRoomCode(connectionId);
        }

        public bool IsPlayerHost(string connectionId)
        {
            return _queryService.IsPlayerHost(connectionId);
        }

        public List<GameRoom> GetAllRooms()
        {
            return _queryService.GetAllRooms();
        }
    }
}