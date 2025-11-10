using System.Collections.Generic;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests
{
    public class GetRoomStatementsReturnsSameStatementsForSameRoom : EndToEndTestBase
    {
        [Fact]
        public void ReturnsSameStatements_ForSameRoom()
        {
            var roomCode = "ROOM-C";

            Context.Statements.AddRange(new List<GameStatement>
            {
                new GameStatement { Id = "S1-C", Text = "Statement 1", Topic = "General" },
                new GameStatement { Id = "S2-C", Text = "Statement 2", Topic = "General" },
            });
            Context.SaveChanges();

            var firstCall = CompatibilityService.GetRoomStatements(roomCode, count: 2);
            var secondCall = CompatibilityService.GetRoomStatements(roomCode, count: 2);

            Assert.Equal(firstCall, secondCall);
        }
    }
}