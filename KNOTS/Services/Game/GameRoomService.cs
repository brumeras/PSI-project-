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
        private readonly RoomQueryService _queryService;

        public GameRoomService()
        {
            _roomRepository = new RoomRepository();
            _playerMappingRepository = new PlayerMappingRepository();

            var codeGenerator = new RoomCodeGenerator();
            _roomManager = new RoomManager(_roomRepository, codeGenerator);
            _playerManager = new PlayerManager(_roomRepository, _playerMappingRepository, _roomManager);
            _queryService = new RoomQueryService(_roomRepository, _playerMappingRepository);
        }
        public void AddPlayer(string connectionId, string username) { _playerMappingRepository.AddPlayer(connectionId, username); }
        public string CreateRoom(string hostConnectionId, string hostUsername) {
            var room = _roomManager.CreateRoom(hostConnectionId, hostUsername);
            _playerMappingRepository.MapPlayerToRoom(hostConnectionId, room.RoomCode);
            return room.RoomCode;
        }
        public JoinRoomResult JoinRoom(string roomCode, string connectionId, string username) { return _playerManager.JoinRoom(roomCode, connectionId, username); }
        public DisconnectedPlayerInfo RemovePlayer(string connectionId) { return _playerManager.RemovePlayer(connectionId);}
        public GameRoom? GetRoomInfo(string roomCode) { return _queryService.GetRoomInfo(roomCode); }
        public List<string> GetRoomPlayerUsernames(string roomCode) { return _queryService.GetRoomPlayerUsernames(roomCode); }
        public string GetPlayerUsername(string connectionId) { return _queryService.GetPlayerUsername(connectionId); }

    }
