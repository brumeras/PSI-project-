using KNOTS.Compability;
using KNOTS.Services.Compability;

namespace KNOTS.Services.Interfaces;

public interface InterfaceCompatibilityCalculator
{
    CompatibilityScore Calculate(string roomCode, string player1, string player2);
    List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames);
    Dictionary<string, PlayerMatchInfo> GetBestMatchesForPlayers(List<CompatibilityScore> allResults);
    PlayerGameStatistics GetPlayerStatistics(string playerUsername, List<CompatibilityScore> allResults);
}