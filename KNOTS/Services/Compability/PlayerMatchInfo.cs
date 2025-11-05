using KNOTS.Compability;

namespace KNOTS.Services.Compability;

public class PlayerMatchInfo {
    /// <summary>
    /// Gets or sets the username of the player’s best match partner.
    /// </summary>
    public string BestMatchPartner { get; set; } = "";
    /// <summary>
    /// Gets or sets the compatibility percentage with the best match partner.
    /// </summary>
    public double BestMatchPercentage { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this player was also the partner’s best match.
    /// </summary>
    public bool WasBestMatchForPartner { get; set; }
    /// <summary>
    /// Gets or sets all compatibility results associated with the player.
    /// </summary>
    public List<CompatibilityScore> AllMatches { get; set; } = new();
}
