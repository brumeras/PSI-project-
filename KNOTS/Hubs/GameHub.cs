﻿using Microsoft.AspNetCore.SignalR;
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

        // Vartotojas prisijungia prie žaidimo
        public async Task JoinGame(string username)
        {
            var connectionId = Context.ConnectionId;
            _gameRoomService.AddPlayer(connectionId, username);
            
            // Pranešame visiem apie naują žaidėją
            await Clients.All.SendAsync("PlayerJoined", username);
            
            // Siunčiame žaidėjui jo unikalų ID
            await Clients.Caller.SendAsync("AssignPlayerId", connectionId);
        }

        // Sukurti naują kambarį
        public async Task CreateRoom(string username)
        {
            var connectionId = Context.ConnectionId;
            var roomCode = _gameRoomService.CreateRoom(connectionId, username);
            
            // Prisijungiam prie šio kambario grupės
            await Groups.AddToGroupAsync(connectionId, $"Room_{roomCode}");
            
            // Pranešame žaidėjui apie sėkmingą kambario sukūrimą
            await Clients.Caller.SendAsync("RoomCreated", roomCode);
        }

        // Prisijungti prie kambario
        public async Task JoinRoom(string roomCode, string username)
        {
            var connectionId = Context.ConnectionId;
            var result = _gameRoomService.JoinRoom(roomCode, connectionId, username);
            
            if (result.Success)
            {
                // Prisijungiam prie kambario grupės
                await Groups.AddToGroupAsync(connectionId, $"Room_{roomCode}");
                
                // Pranešame visiems kambaryje apie naują žaidėją
                await Clients.Group($"Room_{roomCode}").SendAsync("PlayerJoinedRoom", username);
                
                // Siunčiam žaidėjui kambario informaciją
                var roomInfo = _gameRoomService.GetRoomInfo(roomCode);
                await Clients.Caller.SendAsync("JoinedRoom", roomInfo);
            }
            else
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", result.Message);
            }
        }

        // Išsiųsti žaidimo ėjimą/veiksmą
        public async Task SendGameAction(string roomCode, string action, object data)
        {
            var connectionId = Context.ConnectionId;
            var username = _gameRoomService.GetPlayerUsername(connectionId);
            
            // Persiųsti veiksmą visiems kambaryje esantiems žaidėjams
            await Clients.Group($"Room_{roomCode}").SendAsync("GameAction", username, action, data);
        }

        // Atsijungimo valdymas
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var disconnectedInfo = _gameRoomService.RemovePlayer(connectionId);
            
            if (!string.IsNullOrEmpty(disconnectedInfo.RoomCode))
            {
                await Clients.Group($"Room_{disconnectedInfo.RoomCode}")
                    .SendAsync("PlayerLeft", disconnectedInfo.Username);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
