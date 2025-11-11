using KNOTS.Compability;
using KNOTS.Data;
using KNOTS.Services.Compatability;

namespace KNOTS.Services.Compability;

public class SwipeRepository : ISwipeRepository {
    private readonly AppDbContext _context;
    public SwipeRepository(AppDbContext context) {_context = context;}
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
}