using System;

namespace KNOTS.Services;

/// <summary>
/// Represents a user/player in the system, including credentials and game statistics.
/// </summary>
/// <remarks>
/// Implements <see cref="IComparable{User}"/> to allow ranking users based on:
/// - Number of best matches
/// - Average compatibility score
/// - Total games played
/// Implements <see cref="IEquatable{User}"/> to provide equality checks based on username.
/// </remarks>
public class User : IComparable<User>, IEquatable<User> {
    
    /// <summary>Unique username of the player.</summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>Date and time when the user was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>Hashed password for authentication.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Total number of games the user has played.</summary>
    public int TotalGamesPlayed { get; set; }
    
    /// <summary>Average compatibility score across all games.</summary>
    public double AverageCompatibilityScore { get; set; }
    
    /// <summary>Number of times the user was identified as the best match.</summary>
    public int BestMatchesCount { get; set; }

    /// <summary>
    /// Compares this user to another user for ranking purposes.
    /// </summary>
    /// <param name="other">The other user to compare to.</param>
    /// <returns>
    /// Negative if this user ranks higher, positive if lower, zero if equal.
    /// </returns
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
    
    /// <summary>
    /// Determines whether this user is equal to another user based on username.
    /// </summary>
    /// <param name="other">The other user to compare to.</param>
    /// <returns>True if usernames are equal (case-insensitive), otherwise false.</returns>
    public bool Equals(User? other) =>
        other != null && Username.Equals(other.Username, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this user is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is a <see cref="User"/> with the same username.</returns>
    public override bool Equals(object? obj) => Equals(obj as User);
    
    /// <summary>
    /// Returns a hash code based on the username (case-insensitive).
    /// </summary>
    public override int GetHashCode() => Username.ToLowerInvariant().GetHashCode();

    /// <summary>Equality operator based on username.</summary>
    public static bool operator ==(User? left, User? right) => Equals(left, right);
    
    /// <summary>Inequality operator based on username.</summary>
    public static bool operator !=(User? left, User? right) => !Equals(left, right);
}