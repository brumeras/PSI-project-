using KNOTS.Models;
using KNOTS.Compability;

namespace KNOTS.Services.Interfaces;

public interface InterfaceCompatibilityService
{
    List<GameStatement> GetRoomStatements(string roomCode, List<string>? selectedTopics = null, int count = 10);
    bool SaveSwipe(string roomCode, string playerUsername, string statementId, bool swipeRight);
    List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername);
    bool HaveAllPlayersFinished(string roomCode, List<string> playerUsernames, int totalStatements);
    List<CompatibilityScore> CalculateAllCompatibilities(string roomCode, List<string> playerUsernames);
    void SaveGameToHistory(string roomCode, List<string> playerUsernames);
    List<GameHistoryEntry> GetPlayerHistory(string playerUsername);
    void ClearRoomData(string roomCode);
}