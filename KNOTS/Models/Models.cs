using System.ComponentModel.DataAnnotations;

namespace KNOTS.Models
{
    public class GameStatement
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
    
    public class PlayerSwipeRecord
    {
        [Key]
        public int Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string PlayerUsername { get; set; } = string.Empty;
        public string StatementId { get; set; } = string.Empty;
        public string StatementText { get; set; } = string.Empty;
        public bool AgreeWithStatement { get; set; }
        public DateTime SwipedAt { get; set; } = DateTime.Now;
    }
    
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
}