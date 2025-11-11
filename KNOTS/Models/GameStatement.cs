using System.ComponentModel.DataAnnotations;

namespace KNOTS.Models;

public class GameStatement {
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public required string Topic { get; set; }
}