using System;
using System.Linq;
using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

//This test allows to see whether two swipes exist
//and if each site has a correct link to the user and statement. 

public class SwipeIntegrationTest
{
    private AppDbContext temporaryDB1()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Kiekvienam testui nauja DB
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    private void checkSwipes()
    {
        using var context = temporaryDB1();

        var user = new User { Username = "checkas", PasswordHash = "matas" };
        context.Users.Add(user);

        var statement1 = new GameStatement { Id = "2222", Text = "some statement", Topic = "topic1" };
        var statement2 = new GameStatement { Id = "3333", Text = "am am am am", Topic = "topic2" };
        context.Statements.AddRange(statement1, statement2);

        var swipe1 = new PlayerSwipeRecord()
        {
            RoomCode = "room1",
            PlayerUsername = user.Username,
            StatementId = statement1.Id,
            StatementText = statement1.Text,
            AgreeWithStatement = true
        };

        var swipe2 = new PlayerSwipeRecord
        {
            RoomCode = "room1",
            PlayerUsername = user.Username,
            StatementId = statement2.Id,
            StatementText = statement2.Text,
            AgreeWithStatement = false
        };

        context.PlayerSwipes.AddRange(swipe1, swipe2);

        context.SaveChanges();

        var savedSwipes = context.PlayerSwipes
            .Where(s => s.PlayerUsername == "checkas")
            .ToList();

        //check whether the swipes exist
        Assert.Equal(2, savedSwipes.Count);

        //checks if it is with the right statement and user
        Assert.Contains(savedSwipes, s => s.StatementId == "2222" && s.PlayerUsername == "checkas");
        Assert.Contains(savedSwipes, s => s.StatementId == "3333" && s.PlayerUsername == "checkas");
    }
    }
