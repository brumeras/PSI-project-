namespace KNOTS.Services;

/// <summary>
/// Represents a single statement/question in a game.
/// </summary>
/// <remarks>
/// A <see cref="GameStatement"/> is used as the content that players
/// swipe on to indicate agreement or disagreement. Each statement
/// has a unique identifier and the text content.
/// </remarks>
public struct GameStatement {
    
    /// <summary>
    /// Gets or sets the unique identifier of the statement.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the text content of the statement.
    /// </summary>
    public string Text { get; set; }
    
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GameStatement"/> struct with the specified ID and text.
    /// </summary>
    /// <param name="id">The unique identifier of the statement.</param>
    /// <param name="text">The text content of the statement.</param>
    public GameStatement(string id, string text) {
        Id = id;
        Text = text;
    }
}