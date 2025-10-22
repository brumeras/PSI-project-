using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;

    public class GameRoomService {
        private readonly RoomRepository _roomRepository;
        private readonly PlayerMappingRepository _playerMappingRepository;
        private readonly RoomManager _roomManager;
        private readonly PlayerManager _playerManager;
        private readonly GameController _gameController;
        private readonly RoomQueryService _queryService;

        public GameRoomService() {
            _roomRepository = new RoomRepository();
            _playerMappingRepository = new PlayerMappingRepository();
            
            var codeGenerator = new RoomCodeGenerator();
            _roomManager = new RoomManager(_roomRepository, codeGenerator);
            _playerManager = new PlayerManager(_roomRepository, _playerMappingRepository, _roomManager);
            _gameController = new GameController(_roomRepository);
            _queryService = new RoomQueryService(_roomRepository, _playerMappingRepository);
        }
        // Player registration
        public void AddPlayer(string connectionId, string username) { _playerMappingRepository.AddPlayer(connectionId, username); }
        // Room creation
        public string CreateRoom(string hostConnectionId, string hostUsername) {
            var room = _roomManager.CreateRoom(hostConnectionId, hostUsername);
            _playerMappingRepository.MapPlayerToRoom(hostConnectionId, room.RoomCode);
            return room.RoomCode;
        }
        // Room joining
        public JoinRoomResult JoinRoom(string roomCode, string connectionId, string username) { return _playerManager.JoinRoom(roomCode, connectionId, username); }
        // Player management
        public DisconnectedPlayerInfo RemovePlayer(string connectionId) { return _playerManager.RemovePlayer(connectionId);}
        public bool SetPlayerReady(string connectionId, bool isReady) { return _playerManager.SetPlayerReady(connectionId, isReady); }
        // Game flow
        public bool StartGame(string roomCode, List<string> statementIds) { return _gameController.StartGame(roomCode, statementIds); }
        public bool AreAllPlayersReady(string roomCode) { return _gameController.AreAllPlayersReady(roomCode); }
        public List<string> GetActiveStatementIds(string roomCode) { return _gameController.GetActiveStatementIds(roomCode); }
        // Query operations
        public GameRoom? GetRoomInfo(string roomCode) { return _queryService.GetRoomInfo(roomCode); }
        public List<string> GetRoomPlayerUsernames(string roomCode) { return _queryService.GetRoomPlayerUsernames(roomCode); }
        public string GetPlayerUsername(string connectionId) { return _queryService.GetPlayerUsername(connectionId); }
        public string? GetPlayerRoomCode(string connectionId) { return _queryService.GetPlayerRoomCode(connectionId); }
        public bool IsPlayerHost(string connectionId) { return _queryService.IsPlayerHost(connectionId); }
        public List<GameRoom> GetAllRooms() { return _queryService.GetAllRooms(); }
    }
