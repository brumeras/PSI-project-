using System;
using System.Collections.Generic;

namespace KNOTS.Compability;

public struct CompatibilityScore {
    public string Player1 { get; set; }
    public string Player2 { get; set; }
    public int MatchingSwipes { get; set; }
    public int TotalStatements { get; set; }
    public List<string> MatchedStatements { get; set; }

    public CompatibilityScore(string player1, string player2, int matchingSwipes, int totalStatements, List<string> matchedStatements) {
        Player1 = player1;
        Player2 = player2;
        MatchingSwipes = matchingSwipes;
        TotalStatements = totalStatements;
        MatchedStatements = matchedStatements;
    }
    public double Percentage => TotalStatements > 0 
        ? Math.Round((double)MatchingSwipes / TotalStatements * 100, 2)
        : 0;
}