using KNOTS.Compability;

namespace KNOTS.Services.Compability;

public class PlayerMatchInfo {
    public string BestMatchPartner { get; set; } = "";
    public double BestMatchPercentage { get; set; }
    public bool WasBestMatchForPartner { get; set; }
    public List<CompatibilityScore> AllMatches { get; set; } = new();
}
