using KNOTS.Data;
using KNOTS.Models;
using Microsoft.EntityFrameworkCore;

namespace TestProject1.IntegrationTests;

public class IntegrationWithDatabase
{
    private AppDbContext temporaryDB()
    {
        //class which configurates a database (uses the one in project)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            //database in memory (temporary)

            //creates a unique identificator
            //gives it a name

            //every time it is a new one, not dependant to a previous one
            .UseInMemoryDatabase(Guid.NewGuid().ToString())

            //creates a configuration
            .Options;
        return new AppDbContext(options);
    }
    //Basically, this test creates a user
    //Then saves data and checks whether it was saved correctly or not
    //Checks integration between user, gamestatement and playerswiperecord
    [Fact]
    public void testingRelationsWithDB()
    {
        var context = temporaryDB();

        var user = new KNOTS.Services.User { Username = "test1", PasswordHash = "test111" };

        var statement = new GameStatement
        {
            Id = "1111",
            Text = "test1",
            Topic = "testing"
        };

        var swipe = new PlayerSwipeRecord
        {
            RoomCode = "t11",
            PlayerUsername = user.Username,
            StatementId = statement.Id,
            StatementText = statement.Text,
            AgreeWithStatement = true
        };
        
        context.Users.Add(user);
        context.Statements.Add(statement);
        context.PlayerSwipes.Add(swipe);
        context.SaveChanges();
        
        var savedUser = context.Users.FirstOrDefault(u => u.Username == "test1");
        var savedSwipe = context.PlayerSwipes.FirstOrDefault(s => s.PlayerUsername == "test1");

        Assert.NotNull(savedUser);
        Assert.NotNull(savedSwipe);
        Assert.Equal("1111", savedSwipe.StatementId);
    }
}

