using System;

namespace KNOTS.Services;
public class User : IComparable<User>, IEquatable<User>
{
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string PasswordHash { get; set; } = string.Empty;
    public int TotalGamesPlayed { get; set; }
    public double AverageCompatibilityScore { get; set; }
    public int BestMatchesCount { get; set; }

    public int CompareTo(User? other)
    {
        if (other is null) return -1;

        var scoreComparison = other.AverageCompatibilityScore.CompareTo(AverageCompatibilityScore);
        if (scoreComparison != 0) return scoreComparison;

        var bestMatchesComparison = other.BestMatchesCount.CompareTo(BestMatchesCount);
        if (bestMatchesComparison != 0) return bestMatchesComparison;

        var totalGamesComparison = other.TotalGamesPlayed.CompareTo(TotalGamesPlayed);
        if (totalGamesComparison != 0) return totalGamesComparison;

        var usernameComparison = string.Compare(Username, other.Username, StringComparison.OrdinalIgnoreCase);
        return usernameComparison != 0
            ? usernameComparison
            : string.Compare(Username, other.Username, StringComparison.Ordinal);
    }

    public bool Equals(User? other)
    {
        if (other is null) return false;

        return AverageCompatibilityScore.Equals(other.AverageCompatibilityScore)
               && BestMatchesCount == other.BestMatchesCount
               && TotalGamesPlayed == other.TotalGamesPlayed
               && string.Equals(Username, other.Username, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as User);

    public override int GetHashCode()
    {
        return HashCode.Combine(
            AverageCompatibilityScore,
            BestMatchesCount,
            TotalGamesPlayed,
            Username.ToLowerInvariant());
    }
}