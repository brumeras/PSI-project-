using KNOTS.Compability;

namespace KNOTS.Services;
public class GameHistoryEntry {
    public string RoomCode { get; set; } = "";
    public DateTime PlayedDate { get; set; }
    public int TotalPlayers { get; set; }
    public string BestMatchPlayer { get; set; } = "";
    public double BestMatchPercentage { get; set; }
    public List<CompatibilityScore> AllResults { get; set; } = new();
}