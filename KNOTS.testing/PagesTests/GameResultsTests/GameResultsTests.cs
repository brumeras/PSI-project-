using AngleSharp.Dom;
using Bunit;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using KNOTS.Compability;
using KNOTS.Services;
using KNOTS.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using KNOTS.Components;
using KNOTS.Components.Pages;
using Microsoft.AspNetCore.Components.Web;

namespace KNOTS.Tests.Components
{
    public class ResultsComponentTests : TestContext
    {
        private readonly Mock<InterfaceCompatibilityService> _mockCompatibilityService;
        private readonly Mock<IGameRoomService> _mockGameRoomService;
        private readonly Mock<NavigationManager> _mockNavManager;

        public ResultsComponentTests()
        {
            _mockCompatibilityService = new Mock<InterfaceCompatibilityService>();
            _mockGameRoomService = new Mock<IGameRoomService>();
            _mockNavManager = new Mock<NavigationManager>();

            Services.AddSingleton(_mockCompatibilityService.Object);
            Services.AddSingleton(_mockGameRoomService.Object);
            Services.AddSingleton(_mockNavManager.Object);
        }

    

        [Fact]
        public void Component_DisplaysNoResults_WhenNoCompatibilityData()
        {
            // Arrange
            SetupMocksForCalculation(new List<CompatibilityScore>());

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("No Results Available", cut.Markup);
                Assert.Contains("Make sure all players have answered the questions", cut.Markup);
            });
        }

        [Fact]
        public void Component_DisplaysCorrectRankings()
        {
            // Arrange
            var results = CreateMockCompatibilityResults();
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("#1", cut.Markup);
                Assert.Contains("#2", cut.Markup);
                Assert.Contains("#3", cut.Markup);
            });
        }

        [Fact]
        public void Component_HighlightsBestMatch()
        {
            // Arrange
            var results = CreateMockCompatibilityResults();
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var bestMatch = cut.Find(".best-match");
                Assert.NotNull(bestMatch);
            });
        }

        [Fact]
        public void Component_ShowsCorrectScoreBadgeClass_ForHighPercentage()
        {
            // Arrange
            var results = new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 17,
                    TotalStatements = 20,
                    MatchedStatements = new List<string>()
                }
            };
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var badge = cut.Find(".score-badge.high");
                Assert.NotNull(badge);
                Assert.Contains("85%", badge.TextContent);
            });
        }

        [Fact]
        public void Component_ShowsCorrectScoreBadgeClass_ForMediumPercentage()
        {
            // Arrange
            var results = new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 13,
                    TotalStatements = 20,
                    MatchedStatements = new List<string>()
                }
            };
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var badge = cut.Find(".score-badge.medium");
                Assert.NotNull(badge);
            });
        }

        [Fact]
        public void Component_ShowsCorrectScoreBadgeClass_ForLowPercentage()
        {
            // Arrange
            var results = new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 9,
                    TotalStatements = 20,
                    MatchedStatements = new List<string>()
                }
            };
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var badge = cut.Find(".score-badge.low");
                Assert.NotNull(badge);
            });
        }

        [Fact]
        public void Component_ShowsCorrectScoreBadgeClass_ForVeryLowPercentage()
        {
            // Arrange
            var results = new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 5,
                    TotalStatements = 20,
                    MatchedStatements = new List<string>()
                }
            };
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var badge = cut.Find(".score-badge.verylow");
                Assert.NotNull(badge);
            });
        }

        [Fact]
        public void Component_TogglesMatchedStatements_OnButtonClick()
        {
            // Arrange
            var results = new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 16,
                    TotalStatements = 20,
                    MatchedStatements = new List<string> { "Statement 1", "Statement 2" }
                }
            };
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            cut.WaitForAssertion(() =>
            {
                var toggleButton = cut.Find(".btn-toggle");
                Assert.Contains("Show", toggleButton.TextContent);
                
                // Click to show
                toggleButton.Click();
            });

            // Assert - statements should be visible
            cut.WaitForAssertion(() =>
            {
                var statements = cut.Find(".matched-statements");
                Assert.NotNull(statements);
                Assert.Contains("Statement 1", statements.TextContent);
                Assert.Contains("Statement 2", statements.TextContent);
                
                var toggleButton = cut.Find(".btn-toggle");
                Assert.Contains("Hide", toggleButton.TextContent);
            });

            // Click to hide
            cut.Find(".btn-toggle").Click();

            // Assert - statements should be hidden
            cut.WaitForAssertion(() =>
            {
                Assert.Throws<ElementNotFoundException>(() => cut.Find(".matched-statements"));
            });
        }

        [Fact]
        public void Component_DisplaysPersonalMatches_ForCurrentUser()
        {
            // Arrange
            var results = new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 16,
                    TotalStatements = 20,
                    MatchedStatements = new List<string>()
                },
                new CompatibilityScore
                {
                    Player1 = "user3",
                    Player2 = "user1",
                    MatchingSwipes = 14,
                    TotalStatements = 20,
                    MatchedStatements = new List<string>()
                }
            };
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Your Personal Matches", cut.Markup);
                Assert.Contains("You & user2", cut.Markup);
                Assert.Contains("You & user3", cut.Markup);
            });
        }
        
        [Fact]
        public async Task Component_InvokesOnResultsSaved_WhenFinishClicked()
        {
            // Arrange
            var results = CreateMockCompatibilityResults();
            SetupMocksForCalculation(results);
    
            _mockGameRoomService.Setup(s => s.GetRoomPlayerUsernames("TEST123"))
                .Returns(new List<string> { "user1", "user2" });

            bool callbackInvoked = false;
            EventCallback onResultsSaved = EventCallback.Factory.Create(this, () => callbackInvoked = true);

            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1")
                .Add(p => p.OnResultsSaved, onResultsSaved));

            // Act
            var saveButton = cut.Find(".btn-save");
            await saveButton.ClickAsync(new MouseEventArgs());

            // Assert
            Assert.True(callbackInvoked);
        }

        [Fact]
        public void Component_DisplaysProgressBar_WithCorrectWidth()
        {
            // Arrange
            var results = new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 15,
                    TotalStatements = 20,
                    MatchedStatements = new List<string>()
                }
            };
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                var progressBar = cut.Find(".progress-bar-fill");
                Assert.Contains("width: 75%", progressBar.GetAttribute("style"));
                Assert.Contains("15 / 20 matches", progressBar.TextContent);
            });
        }

        [Fact]
        public void Component_DisplaysAllPlayerNames_InResults()
        {
            // Arrange
            var results = CreateMockCompatibilityResults();
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.Contains("user1 & user2", cut.Markup);
                Assert.Contains("user2 & user3", cut.Markup);
                Assert.Contains("user1 & user3", cut.Markup);
            });
        }

        [Fact]
        public void Component_HandlesException_DuringCalculation()
        {
            // Arrange
            _mockGameRoomService.Setup(s => s.GetRoomPlayerUsernames(It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Act & Assert - should not throw
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, "user1"));

            cut.WaitForAssertion(() =>
            {
                // Component should handle the error gracefully
                Assert.NotNull(cut);
            });
        }

        [Fact]
        public void Component_DoesNotShowPersonalMatches_WhenUsernameIsEmpty()
        {
            // Arrange
            var results = CreateMockCompatibilityResults();
            SetupMocksForCalculation(results);

            // Act
            var cut = Render<GameResults>(parameters => parameters
                .Add(p => p.RoomCode, "TEST123")
                .Add(p => p.CurrentUsername, ""));

            // Assert
            cut.WaitForAssertion(() =>
            {
                Assert.DoesNotContain("Your Personal Matches", cut.Markup);
            });
        }
        
        // Helper methods
        private List<CompatibilityScore> CreateMockCompatibilityResults()
        {
            return new List<CompatibilityScore>
            {
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user2",
                    MatchingSwipes = 17,
                    TotalStatements = 20,
                    MatchedStatements = new List<string> { "Statement 1", "Statement 2", "Statement 3" }
                },
                new CompatibilityScore
                {
                    Player1 = "user2",
                    Player2 = "user3",
                    MatchingSwipes = 14,
                    TotalStatements = 20,
                    MatchedStatements = new List<string> { "Statement 4", "Statement 5" }
                },
                new CompatibilityScore
                {
                    Player1 = "user1",
                    Player2 = "user3",
                    MatchingSwipes = 12,
                    TotalStatements = 20,
                    MatchedStatements = new List<string> { "Statement 6" }
                }
            };
        }


        private void SetupMocksForCalculation(List<CompatibilityScore>? results = null)
        {
            var players = new List<string> { "user1", "user2", "user3" };
            
            _mockGameRoomService.Setup(s => s.GetRoomPlayerUsernames(It.IsAny<string>()))
                .Returns(players);

            if (results != null)
            {
                _mockCompatibilityService.Setup(s => s.CalculateAllCompatibilities(
                    It.IsAny<string>(),
                    It.IsAny<List<string>>()))
                    .Returns(results);
            }

            _mockCompatibilityService.Setup(s => s.SaveGameToHistory(
                It.IsAny<string>(),
                It.IsAny<List<string>>()));
        }
    }
}