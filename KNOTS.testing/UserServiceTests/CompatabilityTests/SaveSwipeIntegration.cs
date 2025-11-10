using KNOTS.Models;
using KNOTS.Services;

namespace TestProject1.UserServiceTests.CompatabilityTests;

//BUTINIA PAKEISTI STATEMENT ID TESTUOJANT I TOKI KURIO NERA DB (pirma kart testuojant) - ENSUREDEFAULTSTATEMENTS BY DEFAULT IMETA STATEMENTS, TAI REIK TOKIO KURIO NEIMETA
public class SaveSwipeIntegration : UserServiceTestBase{
    [Fact]
    public void SaveSwipe_AddsSwipe() {
        var service = new CompatibilityService(Context, UserService);
        var statement = new GameStatement { Id = "X1", Text = "statement text", Topic = "General" };
        Context.Statements.Add(statement);
        Context.SaveChanges();
        var res = service.SaveSwipe("room1", "player1", "X1", true);
        Assert.True(res);
        var saved = Context.PlayerSwipes.FirstOrDefault();
        Assert.NotNull(saved);
        Assert.Equal("room1", saved.RoomCode);
        Assert.Equal("player1", saved.PlayerUsername);
        Assert.Equal("X1", saved.StatementId);
        Assert.Equal(true, saved.AgreeWithStatement);
    }
}