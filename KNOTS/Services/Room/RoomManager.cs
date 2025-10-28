using System.Collections.Generic;

namespace KNOTS.Services;

/// <summary>
/// Handles creation, maintenance, and cleanup of active game rooms.
/// </summary>
/// <remarks>
/// This class coordinates room lifecycle management, including room creation,
/// host transfer when players disconnect, and automatic cleanup of empty rooms.
/// </remarks>
public class RoomManager {
    private readonly RoomRepository _roomRepository;
    private readonly RoomCodeGenerator _codeGenerator;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RoomManager"/> class.
    /// </summary>
    /// <param name="roomRepository">The repository used to store and manage active game rooms.</param>
    /// <param name="codeGenerator">The service used to generate unique room codes.</param>
    public RoomManager(RoomRepository roomRepository, RoomCodeGenerator codeGenerator) {
        _roomRepository = roomRepository;
        _codeGenerator = codeGenerator;
    }
    
    /// <summary>
    /// Creates a new game room and assigns the host player.
    /// </summary>
    /// <param name="hostConnectionId">The connection ID of the player creating the room.</param>
    /// <param name="hostUsername">The username of the host player.</param>
    /// <returns>A newly created <see cref="GameRoom"/> instance.</returns>
    /// <remarks>
    /// The room will automatically be registered in the repository and initialized
    /// with a unique room code and a single host player.
    /// </remarks>
    public GameRoom CreateRoom(string hostConnectionId, string hostUsername) {
        var existingCodes = _roomRepository.GetAllRoomCodes();
        var roomCode = _codeGenerator.Generate(existingCodes);

        var room = new GameRoom {
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
    
    /// <summary>
    /// Removes a room from the repository if it has no remaining players.
    /// </summary>
    /// <param name="roomCode">The code of the room to clean up.</param>
    public void CleanupEmptyRoom(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        if (room != null && room.IsEmpty()) { _roomRepository.RemoveRoom(roomCode); }
    }
    
    /// <summary>
    /// Transfers room host status to another player if the current host disconnects.
    /// </summary>
    /// <param name="room">The room where the host transfer should occur.</param>
    /// <param name="disconnectedUsername">The username of the player who disconnected.</param>
    /// <remarks>
    /// If the disconnected player was the host and there are remaining players,
    /// the first player in the list becomes the new host.
    /// </remarks>
    public void TransferHostIfNeeded(GameRoom room, string disconnectedUsername) {
        if (room.Host == disconnectedUsername && !room.IsEmpty()) { room.TransferHost(); }
    }
}