namespace KNOTS.Services.Compability;
public class PlayerGameStatistics {
    public string PlayerUsername { get; set; } = "";
    public int GamesPlayed { get; set; }
    public double AverageCompatibility { get; set; }
    public double BestMatchPercentage { get; set; }
    public bool WasBestMatch { get; set; }
}