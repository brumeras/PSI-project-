namespace KNOTS.Services;

public struct GameStatement {
    public string Id { get; set; }
    public string Text { get; set; }
    public GameStatement(string id, string text) {
        Id = id;
        Text = text;
    }
}