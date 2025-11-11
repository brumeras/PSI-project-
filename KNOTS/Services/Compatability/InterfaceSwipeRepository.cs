using KNOTS.Compability;

namespace KNOTS.Services.Interfaces;

public interface InterfaceSwipeRepository
{
    List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername);
}