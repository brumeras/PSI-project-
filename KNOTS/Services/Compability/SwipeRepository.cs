using KNOTS.Compability;
using KNOTS.Data;

namespace KNOTS.Services.Compability;

/// <summary>
/// Repository layer for accessing player swipe data from database
/// </summary>
public class SwipeRepository {
    private readonly AppDbContext _context;
    
    public SwipeRepository(AppDbContext context) {
        _context = context;
    }
    
    /// <summary>
    /// Gauna visus žaidėjo swipes konkrečiame kambaryje
    /// </summary>
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
    /// Gauna visus swipes kambaryje (visų žaidėjų)
    /// </summary>
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
    /// Patikrina ar žaidėjas užbaigė visus klausimus
    /// </summary>
    public bool HasPlayerFinished(string roomCode, string playerUsername, int requiredCount) {
        var swipeCount = _context.PlayerSwipes
            .Count(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername);
        
        return swipeCount >= requiredCount;
    }
    
    /// <summary>
    /// Gauna unikalius statement ID, į kuriuos žaidėjas atsakė
    /// </summary>
    public List<string> GetPlayerStatementIds(string roomCode, string playerUsername) {
        return _context.PlayerSwipes
            .Where(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername)
            .Select(s => s.StatementId)
            .Distinct()
            .ToList();
    }
}