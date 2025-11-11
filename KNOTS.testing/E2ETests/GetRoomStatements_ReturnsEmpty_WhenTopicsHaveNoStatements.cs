using System.Collections.Generic;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class GetRoomStatementsReturnsEmptyWhenTopicsHaveNoStatements : EndToEndTestBase
{
    [Fact]
    public void GetRoomStatements_ReturnsEmptySet_WhenSelectedTopicsAreMissing()
    {
        SeedStatements(new List<GameStatement>
        {
            new GameStatement { Id = "EDGE-1", Text = "Edge statement", Topic = "General" }
        });

        var statements = CompatibilityService.GetRoomStatements(
            "ROOM-EDGE",
            new List<string> { "Science" }
        );

        Assert.Empty(statements);
    }
}
