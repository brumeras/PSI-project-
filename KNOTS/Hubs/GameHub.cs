using Microsoft.AspNetCore.SignalR;
using KNOTS.Services;

namespace KNOTS.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameRoomService _gameRoomService;
        
        public GameHub(GameRoomService gameRoomService)
        {
            _gameRoomService = gameRoomService;
        }

        // User joins the game
        public async Task JoinGame(string username)
        {
            var connectionId = Context.ConnectionId;
            _gameRoomService.AddPlayer(connectionId, username);
            
            // Send player their unique ID - matches JavaScript "AssignPlayerId"
            await Clients.Caller.SendAsync("AssignPlayerId", connectionId);
            
            Console.WriteLine($"Player {username} joined with ID {connectionId}");
        }

        // Create a new room
        public async Task CreateRoom(string username)
        {
            var connectionId = Context.ConnectionId;
            
            try
            {
                var roomCode = _gameRoomService.CreateRoom(connectionId, username);
                
                // Add to SignalR group (without "Room_" prefix)
                await Groups.AddToGroupAsync(connectionId, roomCode);
                
                // Notify player about successful room creation - matches JavaScript "RoomCreated"
                await Clients.Caller.SendAsync("RoomCreated", roomCode);
                
                Console.WriteLine($"Room {roomCode} created by {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating room: {ex.Message}");
                await Clients.Caller.SendAsync("JoinRoomFailed", $"Error creating room: {ex.Message}");
            }
        }

        // Join existing room
        public async Task JoinRoom(string roomCode, string username)
        {
            var connectionId = Context.ConnectionId;
            
            try
            {
                var result = _gameRoomService.JoinRoom(roomCode, connectionId, username);
                
                if (result.Success)
                {
                    // Add to SignalR group (without "Room_" prefix)
                    await Groups.AddToGroupAsync(connectionId, roomCode);
                    
                    // Get room information
                    var roomInfo = _gameRoomService.GetRoomInfo(roomCode);
                    
                    if (roomInfo != null)
                    {
                        // Send simplified room info to the joining player
                        var roomData = new
                        {
                            RoomCode = roomInfo.RoomCode,
                            Players = roomInfo.Players.Select(p => p.Username).ToList()
                        };
                        
                        // Matches JavaScript "JoinedRoom"
                        await Clients.Caller.SendAsync("JoinedRoom", roomData);
                        
                        // Notify OTHERS in the room about new player (not the caller)
                        await Clients.OthersInGroup(roomCode).SendAsync("PlayerJoinedRoom", username);
                        
                        Console.WriteLine($"Player {username} joined room {roomCode}");
                    }
                    else
                    {
                        await Clients.Caller.SendAsync("JoinRoomFailed", "Room information not found");
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("JoinRoomFailed", result.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error joining room: {ex.Message}");
                await Clients.Caller.SendAsync("JoinRoomFailed", $"Error: {ex.Message}");
            }
        }

        // Send game action/move
        public async Task SendGameAction(string roomCode, string action, object data)
        {
            var connectionId = Context.ConnectionId;
            var username = _gameRoomService.GetPlayerUsername(connectionId);
            
            // Forward action to all players in the room
            await Clients.Group(roomCode).SendAsync("GameAction", username, action, data);
            
            Console.WriteLine($"Game action from {username} in room {roomCode}: {action}");
        }

        // Handle disconnection
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var disconnectedInfo = _gameRoomService.RemovePlayer(connectionId);
            
            if (!string.IsNullOrEmpty(disconnectedInfo.RoomCode))
            {
                await Clients.Group(disconnectedInfo.RoomCode)
                    .SendAsync("PlayerLeft", disconnectedInfo.Username);
                
                Console.WriteLine($"Player {disconnectedInfo.Username} disconnected from room {disconnectedInfo.RoomCode}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}