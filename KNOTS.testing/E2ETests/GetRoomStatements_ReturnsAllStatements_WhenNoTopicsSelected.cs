using System.Collections.Generic;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests
{
    public class GetRoomStatementsReturnsAllStatementsWhenNoTopicsSelected : EndToEndTestBase
    {
        [Fact]
        public void ReturnsAllStatements_WhenNoTopicsSelected()
        {
            var roomCode = "ROOM-A";

            Context.Statements.AddRange(new List<GameStatement>
            {
                new GameStatement { Id = "S1-A", Text = "Statement 1", Topic = "General" },
                new GameStatement { Id = "S2-A", Text = "Statement 2", Topic = "General" },
                new GameStatement { Id = "S3-A", Text = "Statement 3", Topic = "General" },
            });
            Context.SaveChanges();

            var statements = CompatibilityService.GetRoomStatements(roomCode, count: 3);

            Assert.NotEmpty(statements);
            Assert.Equal(3, statements.Count);
        }
    }
}