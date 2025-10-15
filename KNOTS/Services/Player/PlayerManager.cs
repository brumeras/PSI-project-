namespace KNOTS.Services;

public class PlayerManager {
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
            if (!_roomRepository.TryGetRoom(roomCode, out var room) || room == null) return new JoinRoomResult(false, "Room not found", GameState.Finished);

            var canJoinResult = room.CanJoin(username);
            if (!canJoinResult.Success) return canJoinResult;

            var player = new GamePlayer(connectionId, username);
            room.AddPlayer(player);

            _playerMappingRepository.MapPlayerToRoom(connectionId, roomCode);
            _playerMappingRepository.AddPlayer(connectionId, username);

            return new JoinRoomResult(true, "Successfully connected to a room!", room.State);
        }

        public DisconnectedPlayerInfo RemovePlayer(string connectionId) {
            var hasRoom = _playerMappingRepository.RemovePlayer(connectionId, out var roomCode, out var username);
            
            var disconnectedUsername = username ?? "";
            var disconnectedRoomCode = "";

            if (hasRoom && roomCode != null) {
                disconnectedRoomCode = roomCode;
                var room = _roomRepository.GetRoom(roomCode);
                if (room != null) {
                    room.RemovePlayer(connectionId);
                    _roomManager.TransferHostIfNeeded(room, disconnectedUsername);
                    _roomManager.CleanupEmptyRoom(roomCode);
                }
            }
            return new DisconnectedPlayerInfo(disconnectedUsername, disconnectedRoomCode);
        }
        public bool SetPlayerReady(string connectionId, bool isReady) {
            var roomCode = _playerMappingRepository.GetPlayerRoomCode(connectionId);
            if (roomCode == null) return false;

            var room = _roomRepository.GetRoom(roomCode);
            if (room == null) return false;

            return room.SetPlayerReady(connectionId, isReady);
        }
}