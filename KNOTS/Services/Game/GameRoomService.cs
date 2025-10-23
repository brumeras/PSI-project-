using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services
{
    
    /// <summary>
    /// Provides a high-level service for managing game rooms, players, and game flow.
    /// </summary>
    /// <remarks>
    /// This class acts as a façade that coordinates lower-level components such as
    /// <see cref="RoomManager"/>, <see cref="PlayerManager"/>, and <see cref="GameController"/>.
    /// It handles player registration, room creation, joining, and progression through the game lifecycle.
    /// </remarks>
    public class GameRoomService {
        private readonly RoomRepository _roomRepository;
        private readonly PlayerMappingRepository _playerMappingRepository;
        private readonly RoomManager _roomManager;
        private readonly PlayerManager _playerManager;
        private readonly GameController _gameController;
        private readonly RoomQueryService _queryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameRoomService"/> class
        /// and sets up all required repositories and managers.
        /// </summary>
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
        
        
        /// <summary>
        /// Registers a player connection and associates it with a username.
        /// </summary>
        /// <param name="connectionId">The unique connection ID of the player.</param>
        /// <param name="username">The username chosen by the player.</param>
        public void AddPlayer(string connectionId, string username) { _playerMappingRepository.AddPlayer(connectionId, username); }
        // Room creation
        
        /// <summary>
        /// Creates a new game room and assigns the specified player as its host.
        /// </summary>
        /// <param name="hostConnectionId">The host's connection ID.</param>
        /// <param name="hostUsername">The host's username.</param>
        /// <returns>The newly created room's unique code.</returns>
        public string CreateRoom(string hostConnectionId, string hostUsername) {
            var room = _roomManager.CreateRoom(hostConnectionId, hostUsername);
            _playerMappingRepository.MapPlayerToRoom(hostConnectionId, room.RoomCode);
            return room.RoomCode;
        }
        // Room joining

        /// <summary>
        /// Attempts to join a player to a specified room.
        /// </summary>
        /// <param name="roomCode">The target room's unique code.</param>
        /// <param name="connectionId">The player's connection ID.</param>
        /// <param name="username">The player's username.</param>
        /// <returns>
        /// A <see cref="JoinRoomResult"/> indicating whether the join was successful
        /// and providing contextual information if it was not.
        /// </returns>
        public JoinRoomResult JoinRoom(string roomCode, string connectionId, string username) { return _playerManager.JoinRoom(roomCode, connectionId, username); }
        // Player management
        
        /// <summary>
        /// Removes a player from their associated room and handles disconnection logic.
        /// </summary>
        /// <param name="connectionId">The connection ID of the player being removed.</param>
        /// <returns>Information about the disconnected player and their former room.</returns>
        public DisconnectedPlayerInfo RemovePlayer(string connectionId) { return _playerManager.RemovePlayer(connectionId);}
        
        /// <summary>
        /// Sets the readiness status of a player within their room.
        /// </summary>
        /// <param name="connectionId">The player's connection ID.</param>
        /// <param name="isReady">Whether the player is ready.</param>
        /// <returns><see langword="true"/> if the update was successful; otherwise, <see langword="false"/>.</returns>
        public bool SetPlayerReady(string connectionId, bool isReady) { return _playerManager.SetPlayerReady(connectionId, isReady); }
        // Game flow
        
        /// <summary>
        /// Starts the game in the specified room with the provided list of statement IDs.
        /// </summary>
        /// <param name="roomCode">The unique code of the room.</param>
        /// <param name="statementIds">The list of statement IDs to be used in the game.</param>
        /// <returns><see langword="true"/> if the game successfully started; otherwise, <see langword="false"/>.</returns>
        public bool StartGame(string roomCode, List<string> statementIds) { return _gameController.StartGame(roomCode, statementIds); }
        
        /// <summary>
        /// Checks whether all players in the room are ready to begin.
        /// </summary>
        /// <param name="roomCode">The unique code of the room.</param>
        /// <returns><see langword="true"/> if all players are ready; otherwise, <see langword="false"/>.</returns>
        public bool AreAllPlayersReady(string roomCode) { return _gameController.AreAllPlayersReady(roomCode); }
        
        /// <summary>
        /// Retrieves the list of active statement IDs associated with a given room.
        /// </summary>
        /// <param name="roomCode">The unique code of the room.</param>
        /// <returns>A list of active statement IDs for the room.</returns>
        public List<string> GetActiveStatementIds(string roomCode) { return _gameController.GetActiveStatementIds(roomCode); }
        // Query operations
        
        /// <summary>
        /// Retrieves the current state and details of a specified room.
        /// </summary>
        /// <param name="roomCode">The unique code of the room.</param>
        /// <returns>The <see cref="GameRoom"/> instance, or <see langword="null"/> if not found.</returns>
        public GameRoom? GetRoomInfo(string roomCode) { return _queryService.GetRoomInfo(roomCode); }
       
        
        /// <summary>
        /// Retrieves a list of usernames for all players currently in the specified room.
        /// </summary>
        /// <param name="roomCode">The unique code of the room.</param>
        /// <returns>A list of usernames in the room.</returns>
        public List<string> GetRoomPlayerUsernames(string roomCode) { return _queryService.GetRoomPlayerUsernames(roomCode); }
       
        /// <summary>
        /// Gets the username associated with a given player connection ID.
        /// </summary>
        /// <param name="connectionId">The player's connection ID.</param>
        /// <returns>The player's username.</returns>
        public string GetPlayerUsername(string connectionId) { return _queryService.GetPlayerUsername(connectionId); }
        
        /// <summary>
        /// Gets the room code associated with a given player connection ID.
        /// </summary>
        /// <param name="connectionId">The player's connection ID.</param>
        /// <returns>
        /// The room code if the player is in a room; otherwise, <see langword="null"/>.
        /// </returns>
        public string? GetPlayerRoomCode(string connectionId) { return _queryService.GetPlayerRoomCode(connectionId); }
        
        /// <summary>
        /// Determines whether the player with the specified connection ID is the host of their room.
        /// </summary>
        /// <param name="connectionId">The player's connection ID.</param>
        /// <returns><see langword="true"/> if the player is the host; otherwise, <see langword="false"/>.</returns>
        public bool IsPlayerHost(string connectionId) { return _queryService.IsPlayerHost(connectionId); }
        
        /// <summary>
        /// Retrieves all currently active game rooms.
        /// </summary>
        /// <returns>A list of all existing <see cref="GameRoom"/> instances.</returns>
        public List<GameRoom> GetAllRooms() { return _queryService.GetAllRooms(); }
    }
}