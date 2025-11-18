using KNOTS.Hubs;
using KNOTS.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace TestProject1.Hub_tests;

public class GameActionDataUnit{
    [Fact]
    public void GameActionDataGetWorks(){
        var dataObject = new { X = 10, Y = 20 };
        var actionData = new GameActionData {
            RoomCode = "room1",
            Action = "mmm",
            Data = dataObject
        };
        Assert.Equal("room1", actionData.RoomCode);
        Assert.Equal("mmm", actionData.Action);
        Assert.Same(dataObject, actionData.Data);
    }
}