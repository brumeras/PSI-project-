using System.ComponentModel.DataAnnotations;

namespace KNOTS.Models;

public class GameHistoryRecord
{
    [Key]
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public DateTime PlayedDate { get; set; } = DateTime.Now;
    public int TotalPlayers { get; set; }
    public string PlayerUsernames { get; set; } = string.Empty;
    public string BestMatchPlayer { get; set; } = string.Empty;
    public double BestMatchPercentage { get; set; }
    public string ResultsJson { get; set; } = string.Empty;
}