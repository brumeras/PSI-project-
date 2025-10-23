using System;

namespace KNOTS.Services;

public class User : IComparable<User>, IEquatable<User> {
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string PasswordHash { get; set; } = string.Empty;

    public int TotalGamesPlayed { get; set; }
    public double AverageCompatibilityScore { get; set; }
    public int BestMatchesCount { get; set; }

    public int CompareTo(User? other) {
        if (other == null) return 1;

        int bestMatchComparison = other.BestMatchesCount.CompareTo(this.BestMatchesCount);
        if (bestMatchComparison != 0) return bestMatchComparison;

        int avgScoreComparison = other.AverageCompatibilityScore.CompareTo(this.AverageCompatibilityScore);
        if (avgScoreComparison != 0) return avgScoreComparison;

        int gamesComparison = other.TotalGamesPlayed.CompareTo(this.TotalGamesPlayed);
        if (gamesComparison != 0) return gamesComparison;

        return string.Compare(this.Username, other.Username, StringComparison.OrdinalIgnoreCase);
    }
    public bool Equals(User? other) =>
        other != null && Username.Equals(other.Username, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as User);
    public override int GetHashCode() => Username.ToLowerInvariant().GetHashCode();

    public static bool operator ==(User? left, User? right) => Equals(left, right);
    public static bool operator !=(User? left, User? right) => !Equals(left, right);
}