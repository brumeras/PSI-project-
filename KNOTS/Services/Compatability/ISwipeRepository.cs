using KNOTS.Models;

namespace KNOTS.Services.Compatability;

//sita interfeisa naudojam, nes tokiu budu galim testutoi klase (mockinti) nenaudojant tikros duomenu bazes ar repositoriju
//taip pat su interfeisais galima lengviau plesti logika ir panasiai, nes atskiriam logika nuo duomenu
public interface ISwipeRepository {
    List<PlayerSwipe> GetPlayerSwipes(string roomCode, string username);
}