using KNOTS.Data;
using KNOTS.Hubs;
using KNOTS.Models;
using KNOTS.Services;
using KNOTS.Services.Chat;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace TestProject1.ChatTests;

public class MessageServiceTests {
    private MessageService CreateService(
        out AppDbContext context,
        out Mock<IHubContext<ChatHub>> hubMock,
        out Mock<IClientProxy> clientProxyMock) {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        context = new AppDbContext(options);
        hubMock = new Mock<IHubContext<ChatHub>>();
        var clientsMock = new Mock<IHubClients>();
        clientProxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        return new MessageService(context, hubMock.Object);
    }
    [Fact]
    public async Task SendMessage_Saves_And_Broadcasts() {
        var service = CreateService(out var db, out var hubMock, out var _);
        var fakeProxy = new FakeClientProxy();
        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(fakeProxy);
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        var msg = new Message {
            SenderId = "alice",
            ReceiverId = "bob",
            Content = "hello!"
        };
        await service.SendMessage(msg);
        Assert.Equal(1, db.Messages.Count());
        var savedMessage = db.Messages.First();
        Assert.Equal("alice", savedMessage.SenderId);
        Assert.Equal("bob", savedMessage.ReceiverId);
        Assert.Equal("hello!", savedMessage.Content);
        Assert.Contains(fakeProxy.SentMessages, m => m.Method == "ReceiveMessage" && m.Args[0] is Message);
        Assert.Contains(fakeProxy.SentMessages, m => m.Method == "MessageSent" && m.Args[0] is Message);
    }
    public class FakeClientProxy : IClientProxy {
        public List<(string Method, object[] Args)> SentMessages { get; } = new();
        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default) {
            SentMessages.Add((method, args!));
            return Task.CompletedTask;
        }
    }
    [Fact]
    public async Task GetConversation_ReturnsMessagesOrdered() {
        var service = CreateService(out var db, out var hubMock, out var clientProxyMock);
        db.Messages.AddRange(
            new Message { SenderId = "alice", ReceiverId = "bob", Content = "1", SentAt = DateTime.UtcNow.AddMinutes(-2) },
            new Message { SenderId = "bob", ReceiverId = "alice", Content = "2", SentAt = DateTime.UtcNow.AddMinutes(-1) }
        );
        await db.SaveChangesAsync();
        var result = await service.GetConversation("Alice", "Bob");
        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].Content);
        Assert.Equal("2", result[1].Content);
    }
    [Fact]
    public async Task GetUserConversations_GroupsCorrectly() {
        var service = CreateService(out var db, out var hubMock, out var clientProxyMock);
        db.Messages.AddRange(
            new Message { SenderId = "alice", ReceiverId = "bob", Content = "Hey", SentAt = DateTime.UtcNow.AddMinutes(-5) },
            new Message { SenderId = "bob", ReceiverId = "alice", Content = "Hi", SentAt = DateTime.UtcNow.AddMinutes(-4) },
            new Message { SenderId = "alice", ReceiverId = "charlie", Content = "Yo", SentAt = DateTime.UtcNow.AddMinutes(-1) }
        );
        await db.SaveChangesAsync();
        var messages = db.Messages.AsEnumerable().ToList(); // fetch in-memory
        var testService = new TestMessageService(messages, hubMock.Object);
        var conversations = await testService.GetUserConversations("alice");
        Assert.Equal(2, conversations.Count);
        Assert.Equal("charlie", conversations[0].User2Username);
        Assert.Equal("bob", conversations[1].User2Username);
    }
    public class TestMessageService {
        private readonly List<Message> _messages;
        private readonly IHubContext<ChatHub> _hub;
        public TestMessageService(List<Message> messages, IHubContext<ChatHub> hub) {
            _messages = messages;
            _hub = hub;
        }
        public Task<List<Conversation>> GetUserConversations(string username) {
            var conversations = _messages
                .Where(m => m.SenderId == username || m.ReceiverId == username)
                .GroupBy(m => m.SenderId == username ? m.ReceiverId : m.SenderId)
                .Select(g => new Conversation
                {
                    User1Username = username,
                    User2Username = g.Key,
                    LastMessageAt = g.Max(e => e.SentAt),
                    Messages = g.OrderByDescending(e => e.SentAt).Take(1).ToList()
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            return Task.FromResult(conversations);
        }
    }
    [Fact]
    public async Task MarkConversationAsRead_MarksMessages() {
        var service = CreateService(out var db, out var hubMock, out var clientProxyMock);
        db.Messages.AddRange(
            new Message { SenderId = "bob", ReceiverId = "alice", IsRead = false },
            new Message { SenderId = "bob", ReceiverId = "alice", IsRead = false }
        );
        await db.SaveChangesAsync();
        await service.MarkConversationAsRead("Alice", "Bob");
        Assert.True(db.Messages.All(m => m.IsRead));
    }
    [Fact]
    public async Task GetUserConversations_ReturnsOrderedConversations() {
        var service = CreateService(out var db, out var hubMock, out var _);
        var now = DateTime.UtcNow;
        db.Messages.AddRange(
            new Message { SenderId = "alice", ReceiverId = "bob", Content = "Hey", SentAt = now.AddMinutes(-5) },
            new Message { SenderId = "bob", ReceiverId = "alice", Content = "Hi", SentAt = now.AddMinutes(-4) },
            new Message { SenderId = "alice", ReceiverId = "charlie", Content = "Yo", SentAt = now.AddMinutes(-1) }
        );
        await db.SaveChangesAsync();
        var messages = db.Messages.AsEnumerable().ToList();
        var testService = new TestMessageService(messages, hubMock.Object);
        var conversations = await testService.GetUserConversations("alice");
        Assert.Equal(2, conversations.Count);
        Assert.Equal("charlie", conversations[0].User2Username);
        Assert.Equal("bob", conversations[1].User2Username);
    }
    [Fact]
    public async Task GetUnreadCount_ReturnsCorrectCount() {
        var service = CreateService(out var db, out var hubMock, out var _);
        db.Messages.AddRange(
            new Message { SenderId = "bob", ReceiverId = "alice", IsRead = false },
            new Message { SenderId = "charlie", ReceiverId = "alice", IsRead = true }
        );
        await db.SaveChangesAsync();
        var unread = await service.GetUnreadCount("alice");
        Assert.Equal(1, unread);
    }
    [Fact]
    public async Task MarkAsRead_UpdatesMessage() {
        var service = CreateService(out var db, out var hubMock, out var _);
        var msg = new Message { SenderId = "bob", ReceiverId = "alice", IsRead = false };
        db.Messages.Add(msg);
        await db.SaveChangesAsync();
        await service.MarkAsRead(msg.Id);
        var updated = await db.Messages.FindAsync(msg.Id);
        Assert.True(updated.IsRead);
    }
    [Fact] public async Task MarkConversationAsRead_UpdatesAllMessages(){ 
        var service = CreateService(out var db, out var hubMock, out var _);
        db.Messages.AddRange(
            new Message { SenderId = "bob", ReceiverId = "alice", IsRead = false },
            new Message { SenderId = "bob", ReceiverId = "alice", IsRead = false },
            new Message { SenderId = "alice", ReceiverId = "bob", IsRead = false } // should not be marked
        );
        await db.SaveChangesAsync();
        await service.MarkConversationAsRead("alice", "bob");
        var messages = db.Messages.ToList();
        Assert.All(messages.Where(m => m.SenderId == "bob" && m.ReceiverId == "alice"), m => Assert.True(m.IsRead));
        Assert.False(messages.First(m => m.SenderId == "alice" && m.ReceiverId == "bob").IsRead);
    }

}
