using Bunit;
using KNOTS.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using KNOTS.Services.Interfaces;
using KNOTS.Models;
using KNOTS.Services;
using Microsoft.AspNetCore.Components;

namespace KNOTS.Tests.Integration
{
    public class LeaderboardIntegrationTests : TestContext
    {
        private readonly Mock<InterfaceUserService> _mockUserService;

        public LeaderboardIntegrationTests()
        {
            _mockUserService = new Mock<InterfaceUserService>();
            Services.AddSingleton(_mockUserService.Object);
        }

        [Fact]
        public void Should_Display_Authentication_Warning_When_Not_Authenticated()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(false);

            // Act
            var cut = Render<Leaderboard>();

            // Assert
            var authWarning = cut.Find(".auth-warning");
            Assert.NotNull(authWarning);
            Assert.Contains("Authentication Required", authWarning.TextContent);
            Assert.Contains("Please login to view the leaderboard", authWarning.TextContent);
            
            var loginLink = cut.Find(".btn-login-link");
            Assert.Equal("/", loginLink.GetAttribute("href"));
        }

        [Fact]
        public async Task Should_Display_Leaderboard_With_Users()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.CurrentUser).Returns("testuser");
            _mockUserService.Setup(s => s.GetTotalUsersCount()).Returns(5);
            _mockUserService.Setup(s => s.GetUserRank("testuser")).Returns(2);
            
            var leaderboardData = new List<User>
            {
                new User 
                { 
                    Username = "player1", 
                    TotalGamesPlayed = 10, 
                    BestMatchesCount = 5, 
                    AverageCompatibilityScore = 85.5,
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new User 
                { 
                    Username = "testuser", 
                    TotalGamesPlayed = 8, 
                    BestMatchesCount = 4, 
                    AverageCompatibilityScore = 82.3,
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                new User 
                { 
                    Username = "player3", 
                    TotalGamesPlayed = 6, 
                    BestMatchesCount = 3, 
                    AverageCompatibilityScore = 78.9,
                    CreatedAt = DateTime.Now.AddDays(-10)
                }
            };
            
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(leaderboardData);

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert
            var table = cut.Find(".leaderboard-table");
            Assert.NotNull(table);
            
            var rows = cut.FindAll("tbody tr");
            Assert.Equal(3, rows.Count);
            
            // Check first player
            Assert.Contains("player1", rows[0].TextContent);
            Assert.Contains("10", rows[0].TextContent); // Games played
            Assert.Contains("5", rows[0].TextContent); // Best matches
            
            
            // Check current user has "You" badge
            Assert.Contains("You", rows[1].TextContent);
            var youBadge = rows[1].QuerySelector(".you-badge");
            Assert.NotNull(youBadge);
            
            // Check rank display
            var userRankInfo = cut.Find(".user-rank-info");
            Assert.Contains("#2", userRankInfo.TextContent);
            Assert.Contains("5", userRankInfo.TextContent);
        }

        [Fact]
        public async Task Should_Display_Top_Three_Ranks_With_Special_Styling()
        {
            _mockUserService.Reset();
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.GetTotalUsersCount()).Returns(5);
            
            var leaderboardData = new List<User>
            {
                new User { Username = "first", TotalGamesPlayed = 10, BestMatchesCount = 5, AverageCompatibilityScore = 90, CreatedAt = DateTime.Now },
                new User { Username = "second", TotalGamesPlayed = 9, BestMatchesCount = 4, AverageCompatibilityScore = 85, CreatedAt = DateTime.Now },
                new User { Username = "third", TotalGamesPlayed = 8, BestMatchesCount = 3, AverageCompatibilityScore = 80, CreatedAt = DateTime.Now },
                new User { Username = "fourth", TotalGamesPlayed = 7, BestMatchesCount = 2, AverageCompatibilityScore = 75, CreatedAt = DateTime.Now }
            };
            
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(leaderboardData);

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert
            var topRanks = cut.FindAll("span.rank-1, span.rank-2, span.rank-3");
            Assert.Equal(3, topRanks.Count);
            
            Assert.Contains("1", topRanks[0].TextContent);
            Assert.Contains("2", topRanks[1].TextContent);
            Assert.Contains("3", topRanks[2].TextContent);
        }

        [Fact]
        public async Task Should_Highlight_Current_User_Row()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.CurrentUser).Returns("myuser");
            _mockUserService.Setup(s => s.GetTotalUsersCount()).Returns(2);
            _mockUserService.Setup(s => s.GetUserRank("myuser")).Returns(1);
            
            var leaderboardData = new List<User>
            {
                new User { Username = "myuser", TotalGamesPlayed = 10, BestMatchesCount = 5, AverageCompatibilityScore = 85, CreatedAt = DateTime.Now },
                new User { Username = "other", TotalGamesPlayed = 8, BestMatchesCount = 4, AverageCompatibilityScore = 80, CreatedAt = DateTime.Now }
            };
            
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(leaderboardData);

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert
            var currentUserRow = cut.Find(".current-user-row");
            Assert.NotNull(currentUserRow);
            Assert.Contains("myuser", currentUserRow.TextContent);
        }

        [Fact]
        public async Task Should_Display_No_Data_Message_When_Leaderboard_Empty()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(new List<User>());
            _mockUserService.Setup(s => s.GetTotalUsersCount()).Returns(0);

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert
            var noDataCard = cut.Find(".no-data-card");
            Assert.NotNull(noDataCard);
            Assert.Contains("No Rankings Yet", noDataCard.TextContent);
            Assert.Contains("No players have completed games yet", noDataCard.TextContent);
            
            var playButton = cut.Find(".btn-play");
            Assert.Equal("/game", playButton.GetAttribute("href"));
        }

        [Fact]
        public void Should_Display_Page_Title()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(new List<User>());

            // Act
            var cut = Render<Leaderboard>();

            // Assert
            var pageTitle = cut.Find("h1");
            Assert.Equal("Top Knotters", pageTitle.TextContent);
        }

        [Fact]
        public void Should_Display_Home_Button()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(new List<User>());

            // Act
            var cut = Render<Leaderboard>();

            // Assert
            var homeButton = cut.Find(".btn-home");
            Assert.Equal("/Home", homeButton.GetAttribute("href"));
            Assert.Contains("Home", homeButton.TextContent);
        }

        [Fact]
        public async Task Should_Display_Correct_Table_Headers()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(new List<User> 
            { 
                new User { Username = "test", TotalGamesPlayed = 1, BestMatchesCount = 1, AverageCompatibilityScore = 80, CreatedAt = DateTime.Now } 
            });

            // Act
            var cut = Render<Leaderboard>();
            cut.WaitForState(() => cut.FindAll("th").Count > 0);

            // Assert
            var headers = cut.FindAll("th");
            Assert.Equal(6, headers.Count);
            Assert.Equal("Rank", headers[0].TextContent.Trim());
            Assert.Equal("Username", headers[1].TextContent.Trim());
            Assert.Equal("Games Played", headers[2].TextContent.Trim());
            Assert.Equal("Best Matches", headers[3].TextContent.Trim());
            Assert.Equal("Avg Compatibility", headers[4].TextContent.Trim());
            Assert.Equal("Member Since", headers[5].TextContent.Trim());
        }

        [Fact]
        public async Task Should_Format_Date_Correctly()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            var testDate = new DateTime(2024, 3, 15);
            
            var leaderboardData = new List<User>
            {
                new User 
                { 
                    Username = "test", 
                    TotalGamesPlayed = 1, 
                    BestMatchesCount = 1, 
                    AverageCompatibilityScore = 80, 
                    CreatedAt = testDate 
                }
            };
            
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(leaderboardData);

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert
            var cells = cut.FindAll("tbody td");
            Assert.Contains("Mar 15, 2024", cells[cells.Count - 1].TextContent);
        }

        [Fact]
        public async Task Should_Format_Compatibility_Score_With_One_Decimal()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            
            var leaderboardData = new List<User>
            {
                new User 
                { 
                    Username = "test", 
                    TotalGamesPlayed = 1, 
                    BestMatchesCount = 1, 
                    AverageCompatibilityScore = 85.678, 
                    CreatedAt = DateTime.Now 
                }
            };
            
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(leaderboardData);

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert
            var compatibilityScore = cut.Find(".compatibility-score");
            Assert.Equal("85,7%", compatibilityScore.TextContent);
        }

        [Fact]
        public void Should_Not_Display_User_Rank_Info_When_Not_Authenticated()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(false);

            // Act
            var cut = Render<Leaderboard>();

            // Assert
            var userRankInfoElements = cut.FindAll(".user-rank-info");
            Assert.Empty(userRankInfoElements);
        }

        [Fact]
        public async Task Should_Call_UserService_Methods_On_Initialization()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.CurrentUser).Returns("testuser");
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(new List<User>());
            _mockUserService.Setup(s => s.GetTotalUsersCount()).Returns(0);
            _mockUserService.Setup(s => s.GetUserRank("testuser")).Returns(0);

            // Act
            var cut = Render<Leaderboard>();
            
            // Wait for the component to complete initialization (300ms delay + processing)
            await Task.Delay(400);

            // Assert
            _mockUserService.Verify(s => s.GetLeaderboard(10), Times.Once);
            _mockUserService.Verify(s => s.GetTotalUsersCount(), Times.Once);
            _mockUserService.Verify(s => s.GetUserRank("testuser"), Times.Once);
        }

        [Fact]
        public async Task Should_Display_Match_Badge_With_Correct_Styling()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            
            var leaderboardData = new List<User>
            {
                new User 
                { 
                    Username = "test", 
                    TotalGamesPlayed = 10, 
                    BestMatchesCount = 7, 
                    AverageCompatibilityScore = 85, 
                    CreatedAt = DateTime.Now 
                }
            };
            
            _mockUserService.Setup(s => s.GetLeaderboard(10)).Returns(leaderboardData);

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert
            var matchBadge = cut.Find(".match-badge");
            Assert.NotNull(matchBadge);
            Assert.Equal("7", matchBadge.TextContent);
        }

        [Fact]
        public async Task Should_Handle_Exception_In_LoadLeaderboard_Gracefully()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.GetLeaderboard(10))
                .Throws(new Exception("Test exception"));

            // Act
            var cut = Render<Leaderboard>();
            await Task.Delay(400);

            // Assert - Should display no data card instead of crashing
            var noDataCard = cut.Find(".no-data-card");
            Assert.NotNull(noDataCard);
        }

        [Fact]
        public async Task Should_Request_Top_10_Users_From_Service()
        {
            // Arrange
            _mockUserService.Setup(s => s.IsAuthenticated).Returns(true);
            _mockUserService.Setup(s => s.GetLeaderboard(It.IsAny<int>())).Returns(new List<User>());

            // Act
            var cut = Render<Leaderboard>();
            
            // Wait for the component to complete initialization
            await Task.Delay(400);

            // Assert
            _mockUserService.Verify(s => s.GetLeaderboard(10), Times.Once);
        }
    }
}