using System;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;
    public class GameRoom {
        public string RoomCode { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public List<GamePlayer> Players { get; set; } = new();
        public GameState State { get; set; } = GameState.WaitingForPlayers;
        public int MaxPlayers { get; set; } = 4;
        public JoinRoomResult CanJoin(string username) { return GameRoomExs.CanJoin(this, username);}
        public void AddPlayer(GamePlayer player) {
            Players.Add(player);
            if (Players.Count >= MaxPlayers) State = GameState.InProgress;
        } 
        public bool RemovePlayer(string connectionId) => Players.RemoveAll(p => p.ConnectionId == connectionId) > 0;
        public void TransferHost(){ if (Players.Any()) Host = Players.First().Username; }
        public bool IsEmpty() => Players.Count == 0;
    }
