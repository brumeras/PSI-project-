using KNOTS.Compability;
using KNOTS.Data;

namespace KNOTS.Services.Compability;

/// <summary>
/// Repository layer responsible for accessing and retrieving player swipe data from the database.
/// </summary>
/// <remarks>
/// Provides methods to fetch individual or room-wide swipe data, verify completion status,
/// and obtain answered statement IDs for each player.
/// </remarks>
public class SwipeRepository {
    private readonly AppDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SwipeRepository"/> class.
    /// </summary>
    /// <param name="context">Application database context used for data access.</param>
    public SwipeRepository(AppDbContext context) {
        _context = context;
    }
    
    /// <summary>
    /// Retrieves all swipe responses made by a specific player within a given room.
    /// </summary>
    /// <param name="roomCode">The unique identifier of the room.</param>
    /// <param name="playerUsername">The username of the player whose swipes should be retrieved.</param>
    /// <returns>
    /// A list of <see cref="PlayerSwipe"/> objects representing all recorded swipes for the specified player.
    /// </returns>
    public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) {
        return _context.PlayerSwipes
            .Where(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername)
            .Select(s => new PlayerSwipe(
                s.PlayerUsername,
                s.StatementId,
                s.StatementText,
                s.AgreeWithStatement
            ) { SwipedAt = s.SwipedAt })
            .ToList();
    }
    

    /// <summary>
    /// Retrieves all swipe responses recorded in a specific room by all players.
    /// </summary>
    /// <param name="roomCode">The unique identifier of the room.</param>
    /// <returns>
    /// A list of <see cref="PlayerSwipe"/> objects representing all player swipes within the specified room.
    /// </returns>
    public List<PlayerSwipe> GetRoomSwipes(string roomCode) {
        return _context.PlayerSwipes
            .Where(s => s.RoomCode == roomCode)
            .Select(s => new PlayerSwipe(
                s.PlayerUsername,
                s.StatementId,
                s.StatementText,
                s.AgreeWithStatement
            ) { SwipedAt = s.SwipedAt })
            .ToList();
    }
    
    /// <summary>
    /// Determines whether a player has completed all required swipe actions in a given room.
    /// </summary>
    /// <param name="roomCode">The unique identifier of the room.</param>
    /// <param name="playerUsername">The username of the player to check.</param>
    /// <param name="requiredCount">The total number of required statements for completion.</param>
    /// <returns>
    /// <c>true</c> if the player has completed all required swipes; otherwise, <c>false</c>.
    /// </returns>
    public bool HasPlayerFinished(string roomCode, string playerUsername, int requiredCount) {
        var swipeCount = _context.PlayerSwipes
            .Count(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername);
        
        return swipeCount >= requiredCount;
    }
    
    /// <summary>
    /// Retrieves a list of unique statement IDs that a player has responded to within a specific room.
    /// </summary>
    /// <param name="roomCode">The unique identifier of the room.</param>
    /// <param name="playerUsername">The username of the player whose statement IDs are being retrieved.</param>
    /// <returns>
    /// A list of unique statement ID strings representing the player’s answered statements.
    /// </returns>
    public List<string> GetPlayerStatementIds(string roomCode, string playerUsername) {
        return _context.PlayerSwipes
            .Where(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername)
            .Select(s => s.StatementId)
            .Distinct()
            .ToList();
    }
}