using Bunit;
using Bunit.TestDoubles;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Chat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using KNOTS.Components.Pages;


namespace TestProject1.PagesTests
{
    public class ChatRazorTests : TestContext
    {
        private readonly Mock<IMessageService> _mockMessageService;
        private readonly Mock<InterfaceUserService> _mockUserService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;

        public ChatRazorTests()
        {
            _mockMessageService = new Mock<IMessageService>();
            _mockUserService = new Mock<InterfaceUserService>();
            _mockJSRuntime = new Mock<IJSRuntime>();

            // Register services
            Services.AddSingleton(_mockMessageService.Object);
            Services.AddSingleton(_mockUserService.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
        }

        [Fact]
        public void Component_Renders_WithCorrectStructure()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            Assert.NotNull(cut.Find(".chat-container"));
            Assert.NotNull(cut.Find(".chat-header"));
            Assert.NotNull(cut.Find(".messages-container"));
            Assert.NotNull(cut.Find(".input-container"));
        }

        [Fact]
        public void ChatHeader_DisplaysCorrectUsername()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "johndoe"));

            // Assert
            var header = cut.Find(".chat-header h3");
            Assert.Contains("Chat with johndoe", header.TextContent);
        }

        [Fact]
        public async Task OnInitialized_LoadsMessages_WhenUsernamesAreValid()
        {
            // Arrange
            var testMessages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    SenderId = "testuser",
                    ReceiverId = "matcheduser",
                    Content = "Hello",
                    SentAt = DateTime.Now,
                    IsRead = false
                }
            };

            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation("testuser", "matcheduser"))
                .ReturnsAsync(testMessages);
            _mockMessageService
                .Setup(x => x.MarkConversationAsRead(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            await Task.Delay(100); // Allow async initialization

            // Assert
            _mockMessageService.Verify(
                x => x.GetConversation("testuser", "matcheduser"),
                Times.AtLeastOnce);
        }

        [Fact]
        public void MessagesDisplay_ShowsNoMessagesText_WhenEmpty()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var noMessages = cut.Find(".no-messages");
            Assert.Contains("Start the conversation", noMessages.TextContent);
        }

        [Fact]
        public void MessagesDisplay_RendersMessages_Correctly()
        {
            // Arrange
            var testMessages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    SenderId = "testuser",
                    ReceiverId = "matcheduser",
                    Content = "Hello there",
                    SentAt = DateTime.Now,
                    IsRead = false
                },
                new Message
                {
                    Id = 2,
                    SenderId = "matcheduser",
                    ReceiverId = "testuser",
                    Content = "Hi back",
                    SentAt = DateTime.Now.AddMinutes(1),
                    IsRead = true
                }
            };

            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(testMessages);

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var messages = cut.FindAll(".message");
            Assert.Equal(2, messages.Count);
            Assert.Contains("Hello there", messages[0].TextContent);
            Assert.Contains("Hi back", messages[1].TextContent);
        }

        [Fact]
        public void SentMessages_HaveSentClass()
        {
            // Arrange
            var testMessages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    SenderId = "testuser",
                    ReceiverId = "matcheduser",
                    Content = "My message",
                    SentAt = DateTime.Now,
                    IsRead = false
                }
            };

            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(testMessages);

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var sentMessage = cut.Find(".message.sent");
            Assert.NotNull(sentMessage);
        }

        [Fact]
        public void ReceivedMessages_HaveReceivedClass()
        {
            // Arrange
            var testMessages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    SenderId = "matcheduser",
                    ReceiverId = "testuser",
                    Content = "Their message",
                    SentAt = DateTime.Now,
                    IsRead = false
                }
            };

            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(testMessages);

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var receivedMessage = cut.Find(".message.received");
            Assert.NotNull(receivedMessage);
        }

        [Fact]
        public void SendButton_IsDisabled_WhenMessageIsEmpty()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var sendButton = cut.Find(".send-button");
            Assert.True(sendButton.HasAttribute("disabled"));
        }

        [Fact]
        public void SendButton_IsEnabled_WhenMessageHasContent()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Act
            var input = cut.Find(".message-input");
            // Use Input() to trigger oninput instead of onchange
            input.Input("Test message");

            // Assert
            var sendButton = cut.Find(".send-button");
            Assert.False(sendButton.HasAttribute("disabled"));
        }
        

        [Fact]
        public async Task SendMessage_ClearsInput_AfterSending()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());
            _mockJSRuntime
                .Setup(x => x.InvokeAsync<bool>("sendChatMessage", It.IsAny<object[]>()))
                .ReturnsAsync(true);

            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            var input = cut.Find(".message-input");
            // Use Input() to simulate typing
            input.Input("Test message");

            // Act
            var sendButton = cut.Find(".send-button");
            await sendButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
            await Task.Delay(100);

            // Assert
            input = cut.Find(".message-input");
            Assert.Equal(string.Empty, input.GetAttribute("value") ?? string.Empty);
        }

        [Fact]
        public async Task OnChatMessageReceived_AddsMessage_WhenRelevant()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());
            _mockMessageService
                .Setup(x => x.MarkConversationAsRead(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            var newMessage = new Message
            {
                Id = 10,
                SenderId = "matcheduser",
                ReceiverId = "testuser",
                Content = "New incoming message",
                SentAt = DateTime.Now,
                IsRead = false
            };

            var messageJson = System.Text.Json.JsonSerializer.Serialize(newMessage);

            // Act
            await cut.InvokeAsync(() => cut.Instance.OnChatMessageReceived(messageJson));

            // Assert
            var messages = cut.FindAll(".message");
            Assert.Contains(messages, m => m.TextContent.Contains("New incoming message"));
        }

        [Fact]
        public async Task OnChatMessageReceived_IgnoresMessage_WhenNotRelevant()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            var irrelevantMessage = new Message
            {
                Id = 10,
                SenderId = "otheruser",
                ReceiverId = "anotheruser",
                Content = "Irrelevant message",
                SentAt = DateTime.Now,
                IsRead = false
            };

            var messageJson = System.Text.Json.JsonSerializer.Serialize(irrelevantMessage);

            // Act
            await cut.InvokeAsync(() => cut.Instance.OnChatMessageReceived(messageJson));

            // Assert
            var messages = cut.FindAll(".message");
            Assert.DoesNotContain(messages, m => m.TextContent.Contains("Irrelevant message"));
        }

        [Fact]
        public async Task OnChatMessageReceived_MarksAsRead_WhenReceivedByCurrentUser()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());
            _mockMessageService
                .Setup(x => x.MarkConversationAsRead("testuser", "matcheduser"))
                .Returns(Task.CompletedTask);

            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            var newMessage = new Message
            {
                Id = 10,
                SenderId = "matcheduser",
                ReceiverId = "testuser",
                Content = "Test",
                SentAt = DateTime.Now,
                IsRead = false
            };

            var messageJson = System.Text.Json.JsonSerializer.Serialize(newMessage);

            // Act
            await cut.InvokeAsync(() => cut.Instance.OnChatMessageReceived(messageJson));

            // Assert
            _mockMessageService.Verify(
                x => x.MarkConversationAsRead("testuser", "matcheduser"),
                Times.AtLeastOnce);
        }

        [Fact]
        public void MessageTimestamp_DisplaysCorrectFormat()
        {
            // Arrange
            var testTime = new DateTime(2024, 1, 15, 14, 30, 0);
            var testMessages = new List<Message>
            {
                new Message
                {
                    Id = 1,
                    SenderId = "testuser",
                    ReceiverId = "matcheduser",
                    Content = "Test",
                    SentAt = testTime,
                    IsRead = false
                }
            };

            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(testMessages);

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var timeElement = cut.Find(".message-time");
            Assert.Contains("14:30", timeElement.TextContent);
        }

        [Fact]
        public void HomeButton_LinksToCorrectRoute()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var homeButton = cut.Find(".btn-home");
            Assert.Equal("/Home", homeButton.GetAttribute("href"));
        }

        [Fact]
        public async Task Component_DisposesCorrectly()
        {
            // Arrange
            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Message>());

            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Act & Assert - should not throw
            cut.Dispose();
        }

        [Fact]
        public void Messages_OrderedByTimestamp()
        {
            // Arrange
            var testMessages = new List<Message>
            {
                new Message
                {
                    Id = 2,
                    SenderId = "testuser",
                    ReceiverId = "matcheduser",
                    Content = "Second message",
                    SentAt = DateTime.Now.AddMinutes(2),
                    IsRead = false
                },
                new Message
                {
                    Id = 1,
                    SenderId = "matcheduser",
                    ReceiverId = "testuser",
                    Content = "First message",
                    SentAt = DateTime.Now.AddMinutes(1),
                    IsRead = true
                }
            };

            _mockUserService.SetupGet(x => x.CurrentUser).Returns("testuser");
            _mockMessageService
                .Setup(x => x.GetConversation(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(testMessages);

            // Act
            var cut = Render<ChatRazor>(parameters => parameters
                .Add(p => p.MatchedUsername, "matcheduser"));

            // Assert
            var messages = cut.FindAll(".message-content");
            Assert.Contains("First message", messages[0].TextContent);
            Assert.Contains("Second message", messages[1].TextContent);
        }
    }
}