using System;

namespace KNOTS.Services;

/// <summary>
/// Represents a single swipe (decision) made by a player during a game round.
/// </summary>
/// <remarks>
/// A swipe captures the player's opinion (agree/disagree) about a specific game statement,
/// along with metadata such as the time it occurred. These records are used to calculate
/// compatibility, track progress, and generate statistics.
/// </remarks>
public struct PlayerSwipe {
    
    /// <summary>
    /// Gets or sets the username of the player who made the swipe.
    /// </summary>
    public string PlayerUsername { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the statement that was swiped on.
    /// </summary>
    public string StatementId { get; set; }
    
    /// <summary>
    /// Gets or sets the text of the statement being evaluated.
    /// </summary>
    public string StatementText { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the player agreed with the statement.
    /// </summary>
    public bool AgreeWithStatement { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of when the swipe was made.
    /// </summary>
    public DateTime SwipedAt { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerSwipe"/> struct.
    /// </summary>
    /// <param name="playerUsername">The username of the player making the swipe.</param>
    /// <param name="statementId">The unique identifier of the statement being swiped on.</param>
    /// <param name="statementText">The text of the statement.</param>
    /// <param name="agreeWithStatement">Indicates whether the player agreed with the statement.</param>
    public PlayerSwipe(string playerUsername, string statementId, string statementText, bool agreeWithStatement) {
        PlayerUsername = playerUsername;
        StatementId = statementId;
        StatementText = statementText;
        AgreeWithStatement = agreeWithStatement;
        SwipedAt = DateTime.Now;
    }
}
