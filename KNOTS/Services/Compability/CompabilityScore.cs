namespace KNOTS.Compability;
/// <summary>
/// Represents a compatibility score between two players based on their matching swipes.
/// </summary>
/// <remarks>
/// The score includes how many statements both players agreed or disagreed on,
/// the total number of statements compard and the percentage match.
/// </remarks>
public struct CompatibilityScore {
    /// <summary>
    /// Username of the first player.
    /// </summary>
    public string Player1 { get; set; }
    
    /// <summary>
    /// Username of the second player.
    /// </summary>
    public string Player2 { get; set; }
    
    /// <summary>
    /// Number of statements where both players responded the same way.
    /// </summary>
    public int MatchingSwipes { get; set; }
    
    /// <summary>
    /// Total number of statements compared between the two players.
    /// </summary>
    public int TotalStatements { get; set; }
    
    /// <summary>
    /// List of statement texts where both players matched in their responses.
    /// </summary>
    public List<string> MatchedStatements { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CompatibilityScore"/> struct.
    /// </summary>
    public CompatibilityScore(string player1, string player2, int matchingSwipes, int totalStatements, List<string> matchedStatements) {
        Player1 = player1;
        Player2 = player2;
        MatchingSwipes = matchingSwipes;
        TotalStatements = totalStatements;
        MatchedStatements = matchedStatements;
    }

    /// <summary>
    /// The percentage of matching swipes between the two players (0–100).
    /// </summary>
    public double Percentage => TotalStatements > 0 
        ? Math.Round((double)MatchingSwipes / TotalStatements * 100, 2)
        : 0;
}