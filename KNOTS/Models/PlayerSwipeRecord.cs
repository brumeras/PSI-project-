using System.ComponentModel.DataAnnotations;

namespace KNOTS.Models;

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