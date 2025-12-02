using Bunit;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using KNOTS.Services;
using KNOTS.Services.Chat;
using KNOTS.Services.Interfaces;
using KNOTS.Models;
using KNOTS.Compability;
using KNOTS.Components.Pages;
using Microsoft.AspNetCore.Components;

namespace KNOTS.Tests.Integration
{
    public class HomeIntegrationTests : TestContext, IDisposable
    {
        private readonly Mock<InterfaceUserService> _mockUserService;
        private readonly Mock<InterfaceCompatibilityService> _mockCompatibilityService;
        private readonly Mock<IMessageService> _mockMessageService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<NavigationManager> _mockNavigationManager;

        public HomeIntegrationTests()
        {
            _mockUserService = new Mock<InterfaceUserService>();
            _mockCompatibilityService = new Mock<InterfaceCompatibilityService>();
            _mockMessageService = new Mock<IMessageService>();
            _mockJSRuntime = new Mock<IJSRuntime>();
            _mockNavigationManager = new Mock<NavigationManager>();

            Services.AddSingleton(_mockUserService.Object);
            Services.AddSingleton(_mockCompatibilityService.Object);
            Services.AddSingleton(_mockMessageService.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(_mockNavigationManager.Object);
        }

        [Fact]
        public void Home_WhenNotAuthenticated_ShowsLoginPrompt()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(false);

            // Act
            var cut = Render<Home>();

            // Assert
            var heading = cut.Find("h3");
            Assert.Contains("You are not logged in", heading.TextContent);
            
            var link = cut.Find(".link-login");
            Assert.Contains("Go to login page", link.TextContent);
        }

        [Fact]
        public void Home_WhenAuthenticated_ShowsWelcomeMessage()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            SetupEmptyConversations();

            // Act
            var cut = Render<Home>();

            // Assert
            var welcomeTitle = cut.Find(".welcome-title");
            Assert.Contains("Welcome, TestUser", welcomeTitle.TextContent);
        }

        [Fact]
        public void Home_ShowsNavigationWithAllLinks()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            SetupEmptyConversations();

            // Act
            var cut = Render<Home>();

            // Assert
            var navLinks = cut.FindAll(".nav-link");
            Assert.Equal(3, navLinks.Count);
            Assert.Contains("Home", navLinks[0].TextContent);
            Assert.Contains("My Activity", navLinks[1].TextContent);
            Assert.Contains("View Top Knotters", navLinks[2].TextContent);
        }

        [Fact]
        public async Task Home_OnInitialized_LoadsUserContacts()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            
            var conversations = new List<Conversation>
            {
                CreateTestConversation("TestUser", "Partner1", "Hello!", DateTime.Now)
            };
            
            _mockMessageService.Setup(x => x.GetUserConversations("TestUser"))
                .ReturnsAsync(conversations);
            
            SetupCompatibilityHistory("TestUser", "Partner1", 85.5);

            // Act
            var cut = Render<Home>();
            await Task.Delay(100); // Allow async operations to complete

            // Assert
            _mockMessageService.Verify(x => x.GetUserConversations("TestUser"), Times.Once);
            var chatCards = cut.FindAll(".chat-card");
            Assert.Single(chatCards);
        }

        [Fact]
        public async Task Home_DisplaysChatContacts_WithCorrectInformation()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            
            var conversations = new List<Conversation>
            {
                CreateTestConversation("TestUser", "Partner1", "Hello there!", DateTime.Now.AddHours(-1))
            };
            
            _mockMessageService.Setup(x => x.GetUserConversations("TestUser"))
                .ReturnsAsync(conversations);
            
            SetupCompatibilityHistory("TestUser", "Partner1", 92.3);

            // Act
            var cut = Render<Home>();
            await Task.Delay(100);

            // Assert
            var chatCard = cut.Find(".chat-card");
            var chatName = chatCard.QuerySelector(".chat-name");
            var compatChip = chatCard.QuerySelector(".compat-chip");
            var chatPreview = chatCard.QuerySelector(".chat-preview");
            
            Assert.NotNull(chatName);
            Assert.Contains("Partner1", chatName.TextContent);
            Assert.NotNull(compatChip);
            Assert.Contains("92% match", compatChip.TextContent);
            Assert.NotNull(chatPreview);
            Assert.Contains("Hello there!", chatPreview.TextContent);
        }

        [Fact]
        public async Task Home_ShowsUnreadIndicator_ForUnreadMessages()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            
            var conversations = new List<Conversation>
            {
                CreateTestConversation("Partner1", "TestUser", "Unread message", DateTime.Now, isRead: false)
            };
            
            _mockMessageService.Setup(x => x.GetUserConversations("TestUser"))
                .ReturnsAsync(conversations);
            
            SetupCompatibilityHistory("TestUser", "Partner1", 75.0);

            // Act
            var cut = Render<Home>();
            await Task.Delay(100);

            // Assert
            var chatCard = cut.Find(".chat-card");
            Assert.Contains("unread", chatCard.ClassList);
            
            var newPill = cut.Find(".pill-new");
            Assert.Contains("New", newPill.TextContent);
            
            var unreadChip = cut.Find(".unread-chip");
            Assert.Contains("1 new", unreadChip.TextContent);
        }

        [Fact]
        public async Task Home_ShowsEmptyState_WhenNoChats()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            SetupEmptyConversations();

            // Act
            var cut = Render<Home>();
            await Task.Delay(100);

            // Assert
            var emptyState = cut.Find(".chat-empty");
            var heading = emptyState.QuerySelector("h3");
            var paragraph = emptyState.QuerySelector("p");
            
            Assert.NotNull(heading);
            Assert.Contains("No chats yet", heading.TextContent);
            Assert.NotNull(paragraph);
            Assert.Contains("Play a game to meet new players", paragraph.TextContent);
        }
        
        

      
        
        [Fact]
        public async Task Home_LoadContacts_HandlesErrorGracefully()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            
            _mockMessageService.Setup(x => x.GetUserConversations("TestUser"))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var cut = Render<Home>();
            await Task.Delay(100);

            // Assert
            var errorElement = cut.Find(".chat-error");
            Assert.Contains("couldn't load your chats", errorElement.TextContent);
        }

        [Fact]
        public async Task Home_TruncatesLongMessagePreview()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            
            var longMessage = new string('A', 100);
            var conversations = new List<Conversation>
            {
                CreateTestConversation("TestUser", "Partner1", longMessage, DateTime.Now)
            };
            
            _mockMessageService.Setup(x => x.GetUserConversations("TestUser"))
                .ReturnsAsync(conversations);
            
            SetupCompatibilityHistory("TestUser", "Partner1", 80.0);

            // Act
            var cut = Render<Home>();
            await Task.Delay(100);

            // Assert
            var preview = cut.Find(".chat-preview");
            Assert.True(preview.TextContent.Length <= 83); // 80 chars + "..."
            Assert.EndsWith("...", preview.TextContent);
        }

        [Fact]
        public async Task Home_DisplaysContactsWithoutMessages_FromCompatibilityHistory()
        {
            // Arrange
            _mockUserService.Setup(x => x.IsAuthenticated).Returns(true);
            _mockUserService.Setup(x => x.CurrentUser).Returns("TestUser");
            
            _mockMessageService.Setup(x => x.GetUserConversations("TestUser"))
                .ReturnsAsync(new List<Conversation>());
            
            SetupCompatibilityHistory("TestUser", "Partner1", 95.0);

            // Act
            var cut = Render<Home>();
            await Task.Delay(100);

            // Assert
            var chatCard = cut.Find(".chat-card");
            var chatName = chatCard.QuerySelector(".chat-name");
            var compatChip = chatCard.QuerySelector(".compat-chip");
            var chatPreview = chatCard.QuerySelector(".chat-preview");
            
            Assert.NotNull(chatName);
            Assert.Contains("Partner1", chatName.TextContent);
            Assert.NotNull(compatChip);
            Assert.Contains("95% match", compatChip.TextContent);
            Assert.NotNull(chatPreview);
            Assert.Contains("No messages yet", chatPreview.TextContent);
        }

        // Helper Methods

        private void SetupEmptyConversations()
        {
            _mockMessageService.Setup(x => x.GetUserConversations(It.IsAny<string>()))
                .ReturnsAsync(new List<Conversation>());
            
            _mockCompatibilityService.Setup(x => x.GetPlayerHistory(It.IsAny<string>()))
                .Returns(new List<GameHistoryEntry>());
        }

        private Conversation CreateTestConversation(
            string user1, 
            string user2, 
            string messageContent, 
            DateTime timestamp,
            bool isRead = true)
        {
            return new Conversation
            {
                User1Username = user1,
                User2Username = user2,
                Messages = new List<Message>
                {
                    new Message
                    {
                        SenderId = user1,
                        ReceiverId = user2,
                        Content = messageContent,
                        SentAt = timestamp,
                        IsRead = isRead
                    }
                }
            };
        }

        private void SetupCompatibilityHistory(string currentUser, string partnerUser, double score)
        {
            // Ensure your test file has: using KNOTS.Compability;
            // CompatibilityScore.Percentage is computed from MatchingSwipes / TotalStatements.
            const int totalStatements = 100;
            var matchingSwipes = (int)Math.Round(score / 100.0 * totalStatements);
        
            var historyEntry = new GameHistoryEntry
            {
                AllResults = new List<CompatibilityScore>
                {
                    new CompatibilityScore
                    {
                        Player1 = currentUser,
                        Player2 = partnerUser,
                        MatchingSwipes = matchingSwipes,
                        TotalStatements = totalStatements,
                        MatchedStatements = new List<string>()
                    }
                }
            };
        
            _mockCompatibilityService.Setup(x => x.GetPlayerHistory(currentUser))
                .Returns(new List<GameHistoryEntry> { historyEntry });
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}