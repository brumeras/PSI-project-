using System;

namespace KNOTS.Services;
public class User
{
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string PasswordHash { get; set; } = string.Empty;
    public int TotalGamesPlayed { get; set; }
    public double AverageCompatibilityScore { get; set; }
    public int BestMatchesCount { get; set; }
}