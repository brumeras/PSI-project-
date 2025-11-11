using System.Linq;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class GetRoomStatementsReturnsDefaultSetForUnknownRoom : EndToEndTestBase
{
    [Fact]
    public void GetRoomStatements_ReturnsDefaultBatchForNewRoom()
    {
        ResetRoomStatementCache();

        var statements = CompatibilityService.GetRoomStatements("ROOM-NEW");

        Assert.Equal(10, statements.Count);
        Assert.True(statements.All(s => !string.IsNullOrWhiteSpace(s.Id)));
    }
}
