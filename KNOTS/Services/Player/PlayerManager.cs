namespace KNOTS.Services;

/// <summary>
/// Manages player-related actions within game rooms, such as joining, leaving, and readiness updates.
/// </summary>
/// <remarks>
/// The <see cref="PlayerManager"/> coordinates player lifecycle events between
/// the <see cref="RoomRepository"/>, <see cref="PlayerMappingRepository"/>, and <see cref="RoomManager"/>.
/// </remarks>
public class PlayerManager {
        private readonly RoomRepository _roomRepository;
        private readonly PlayerMappingRepository _playerMappingRepository;
        private readonly RoomManager _roomManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerManager"/> class.
        /// </summary>
        /// <param name="roomRepository">The repository responsible for storing and retrieving room data.</param>
        /// <param name="playerMappingRepository">The repository managing mappings between players and their rooms.</param>
        /// <param name="roomManager">The room manager responsible for room state and host transfers.</param>
        public PlayerManager(
            RoomRepository roomRepository,
            PlayerMappingRepository playerMappingRepository,
            RoomManager roomManager)
        {
            _roomRepository = roomRepository;
            _playerMappingRepository = playerMappingRepository;
            _roomManager = roomManager;
        }

        
        /// <summary>
        /// Attempts to add a player to an existing room.
        /// </summary>
        /// <param name="roomCode">The unique code of the room the player wants to join.</param>
        /// <param name="connectionId">The player's unique connection identifier.</param>
        /// <param name="username">The username of the player attempting to join.</param>
        /// <returns>
        /// A <see cref="JoinRoomResult"/> indicating whether the join attempt was successful,
        /// along with a descriptive message and the current <see cref="GameState"/> of the room.
        /// </returns>
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

        /// <summary>
        /// Removes a player from their room and updates room state if necessary.
        /// </summary>
        /// <param name="connectionId">The connection identifier of the player being removed.</param>
        /// <returns>
        /// A <see cref="DisconnectedPlayerInfo"/> object containing information about
        /// the disconnected player and their former room.
        /// </returns>
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
        
        /// <summary>
        /// Updates a player's readiness status in their current room.
        /// </summary>
        /// <param name="connectionId">The unique connection identifier of the player.</param>
        /// <param name="isReady">A boolean value indicating whether the player is ready.</param>
        /// <returns><see langword="true"/> if the readiness state was updated successfully; otherwise, <see langword="false"/>.</returns>
        public bool SetPlayerReady(string connectionId, bool isReady) {
            var roomCode = _playerMappingRepository.GetPlayerRoomCode(connectionId);
            if (roomCode == null) return false;

            var room = _roomRepository.GetRoom(roomCode);
            if (room == null) return false;

            return room.SetPlayerReady(connectionId, isReady);
        }
}