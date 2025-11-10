using System.Collections.Generic;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests
{
    public class GetRoomStatementsFiltersBySelectedTopics : EndToEndTestBase
    {
        [Fact]
        public void FiltersBySelectedTopics()
        {
            var roomCode = "ROOM-B";

            Context.Statements.AddRange(new List<GameStatement>
            {
                new GameStatement { Id = "S1-B", Text = "I love nature", Topic = "Nature" },
                new GameStatement { Id = "S2-B", Text = "I enjoy hiking", Topic = "Nature" },
                new GameStatement { Id = "S3-B", Text = "I love technology", Topic = "Tech" },
            });
            Context.SaveChanges();

            var topics = new List<string> { "Nature" };
            var statements = CompatibilityService.GetRoomStatements(roomCode, selectedTopics: topics, count: 2);

            Assert.NotEmpty(statements);
            Assert.All(statements, s => Assert.Equal("Nature", s.Topic));
            Assert.Equal(2, statements.Count);
        }
    }
}