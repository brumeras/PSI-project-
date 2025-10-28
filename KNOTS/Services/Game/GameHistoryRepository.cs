using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;

/// <summary>
/// Handles persistent storage and getting game history data.
/// </summary>
/// <remarks>
/// Uses a <see cref="JsonFileRepository{T}"/> to manage serialized game history entries.
/// Provides methods to save new game results and query historical games.
/// </remarks>
public class GameHistoryRepository {
    private readonly JsonFileRepository<List<GameHistoryEntry>> _fileRepository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameHistoryRepository"/> class.
    /// </summary>
    /// <param name="fileRepository">
    /// The JSON file repository used to store and load <see cref="GameHistoryEntry"/> data.
    /// </param>
    public GameHistoryRepository(JsonFileRepository<List<GameHistoryEntry>> fileRepository) { _fileRepository = fileRepository; }
    
    /// <summary>
    /// Saves a new game history entry to persistent storage.
    /// </summary>
    /// <param name="entry">The <see cref="GameHistoryEntry"/> to be saved.</param>
    public void Save(GameHistoryEntry entry) {
        var history = _fileRepository.Load();
        history.Add(entry);
        _fileRepository.Save(history);
    }
    
    /// <summary>
    /// Retrieves all past games that involved a specific player.
    /// </summary>
    /// <param name="playerUsername">The username of the player.</param>
    /// <returns>
    /// A list of <see cref="GameHistoryEntry"/> objects sorted by most recent first.
    /// </returns>
    public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) {
        var allHistory = _fileRepository.Load();
        return allHistory
            .Where(h => h.AllResults.Any(r => r.Player1 == playerUsername || r.Player2 == playerUsername))
            .OrderByDescending(h => h.PlayedDate)
            .ToList();
    }
    
    /// <summary>
    /// Retrieves all recorded game history entries.
    /// </summary>
    /// <returns>
    /// A list of <see cref="GameHistoryEntry"/> objects sorted by most recent first.
    /// </returns>
    public List<GameHistoryEntry> GetAll() { return _fileRepository.Load().OrderByDescending(h => h.PlayedDate).ToList(); }
}