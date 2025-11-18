using KNOTS.Compability;
using KNOTS.Data;
<<<<<<< HEAD
using KNOTS.Services.Compatability;

namespace KNOTS.Services.Compability;

public class SwipeRepository : ISwipeRepository {
=======
using KNOTS.Services.Interfaces;

namespace KNOTS.Services.Compability;

public class SwipeRepository : InterfaceSwipeRepository
{
>>>>>>> DependencyInjection
    private readonly AppDbContext _context;
    
    public SwipeRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername)
    {
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