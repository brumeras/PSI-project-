using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using KNOTS.Compability;
using KNOTS.Models;
using KNOTS.Testing;
using Xunit;

namespace TestProject1.E2ETests;

public class GetPlayerHistory_ReturnsOnlySelectedUserGames : EndToEndTestBase
{
    [Fact]
    public void ReturnsOnlyGamesThatIncludeRequestedPlayer()
    {
        const string targetPlayer = "target_player";
        const string sharedPlayer = "shared_player";
        const string otherPlayer = "other_player";
        const string sharedRoom = "ROOM-HISTORY-1";
        const string otherRoom = "ROOM-HISTORY-2";

        RegisterTestUsers(targetPlayer, sharedPlayer);

        Context.Statements.AddRange(new List<GameStatement>
        {
            new GameStatement { Id = "STAT-1", Text = "Statement 1", Topic = "General" },
            new GameStatement { Id = "STAT-2", Text = "Statement 2", Topic = "General" }
        });
        Context.SaveChanges();

        Context.PlayerSwipes.AddRange(new List<PlayerSwipeRecord>
        {
            new PlayerSwipeRecord
            {
                RoomCode = sharedRoom,
                PlayerUsername = targetPlayer,
                StatementId = "STAT-1",
                StatementText = "Statement 1",
                AgreeWithStatement = true
            },
            new PlayerSwipeRecord
            {
                RoomCode = sharedRoom,
                PlayerUsername = targetPlayer,
                StatementId = "STAT-2",
                StatementText = "Statement 2",
                AgreeWithStatement = false
            },
            new PlayerSwipeRecord
            {
                RoomCode = sharedRoom,
                PlayerUsername = sharedPlayer,
                StatementId = "STAT-1",
                StatementText = "Statement 1",
                AgreeWithStatement = true
            },
            new PlayerSwipeRecord
            {
                RoomCode = sharedRoom,
                PlayerUsername = sharedPlayer,
                StatementId = "STAT-2",
                StatementText = "Statement 2",
                AgreeWithStatement = false
            }
        });

        Context.PlayerSwipes.AddRange(new List<PlayerSwipeRecord>
        {
            new PlayerSwipeRecord
            {
                RoomCode = otherRoom,
                PlayerUsername = sharedPlayer,
                StatementId = "STAT-1",
                StatementText = "Statement 1",
                AgreeWithStatement = true
            },
            new PlayerSwipeRecord
            {
                RoomCode = otherRoom,
                PlayerUsername = sharedPlayer,
                StatementId = "STAT-2",
                StatementText = "Statement 2",
                AgreeWithStatement = true
            },
            new PlayerSwipeRecord
            {
                RoomCode = otherRoom,
                PlayerUsername = otherPlayer,
                StatementId = "STAT-1",
                StatementText = "Statement 1",
                AgreeWithStatement = false
            },
            new PlayerSwipeRecord
            {
                RoomCode = otherRoom,
                PlayerUsername = otherPlayer,
                StatementId = "STAT-2",
                StatementText = "Statement 2",
                AgreeWithStatement = false
            }
        });

        Context.SaveChanges();

        CompatibilityService.SaveGameToHistory(sharedRoom, new List<string> { targetPlayer, sharedPlayer });
        CompatibilityService.SaveGameToHistory(otherRoom, new List<string> { sharedPlayer, otherPlayer });

        var history = CompatibilityService.GetPlayerHistory(targetPlayer);

        Assert.Single(history);
        var entry = history[0];

        var storedRecord = Context.GameHistory.Single(record => record.RoomCode == sharedRoom);
        var expectedResults = JsonSerializer.Deserialize<List<CompatibilityScore>>(storedRecord.ResultsJson) ?? new List<CompatibilityScore>();

        Assert.Equal(storedRecord.RoomCode, entry.RoomCode);
        Assert.Equal(storedRecord.TotalPlayers, entry.TotalPlayers);
        Assert.Equal(storedRecord.BestMatchPlayer, entry.BestMatchPlayer);
        Assert.Equal(storedRecord.BestMatchPercentage, entry.BestMatchPercentage);

        var expectedPairs = expectedResults
            .Select(result => (result.Player1, result.Player2, result.Percentage))
            .ToList();
        var actualPairs = entry.AllResults
            .Select(result => (result.Player1, result.Player2, result.Percentage))
            .ToList();
        Assert.Equal(expectedPairs, actualPairs);

        Assert.DoesNotContain(history, h => h.RoomCode == otherRoom);
    }
}
