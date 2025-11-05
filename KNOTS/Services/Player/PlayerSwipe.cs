using System;

namespace KNOTS.Services;
public struct PlayerSwipe {
    public string PlayerUsername { get; set; }
    public string StatementId { get; set; }
    public string StatementText { get; set; }
    public bool AgreeWithStatement { get; set; }
    public DateTime SwipedAt { get; set; }
    public PlayerSwipe(string playerUsername, string statementId, string statementText, bool agreeWithStatement) {
        PlayerUsername = playerUsername;
        StatementId = statementId;
        StatementText = statementText;
        AgreeWithStatement = agreeWithStatement;
        SwipedAt = DateTime.Now;
    }
}
