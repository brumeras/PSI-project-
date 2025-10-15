namespace KNOTS.Services;

public class GameHistoryRepository {
    private readonly JsonFileRepository<List<GameHistoryEntry>> _fileRepository;
    public GameHistoryRepository(JsonFileRepository<List<GameHistoryEntry>> fileRepository) { _fileRepository = fileRepository; }
    public void Save(GameHistoryEntry entry) {
        var history = _fileRepository.Load();
        history.Add(entry);
        _fileRepository.Save(history);
    }
    public List<GameHistoryEntry> GetPlayerHistory(string playerUsername) {
        var allHistory = _fileRepository.Load();
        return allHistory
            .Where(h => h.AllResults.Any(r => r.Player1 == playerUsername || r.Player2 == playerUsername))
            .OrderByDescending(h => h.PlayedDate)
            .ToList();
    }
    public List<GameHistoryEntry> GetAll() { return _fileRepository.Load().OrderByDescending(h => h.PlayedDate).ToList(); }
}