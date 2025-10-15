namespace KNOTS.Services;

    public class GameRoom
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public List<GamePlayer> Players { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public GameState State { get; set; } = GameState.WaitingForPlayers;
        public int MaxPlayers { get; set; } = 4;
        public List<string> ActiveStatementIds { get; set; } = new();

        // Domain logic methods 
        public JoinRoomResult CanJoin(string username)
        {
            if (State != GameState.WaitingForPlayers) return new JoinRoomResult(false, "Game already in progress", State);

            if (Players.Count >= MaxPlayers) return new JoinRoomResult(false, "Room is full", State);

            if (Players.Any(p => p.Username == username)) return new JoinRoomResult(false, "Username already taken in this room", State);

            return new JoinRoomResult(true, "Can join", State);
        }

        public void AddPlayer(GamePlayer player) {
            Players.Add(player);
            
            if (Players.Count >= MaxPlayers) State = GameState.InProgress;
        }

        public bool RemovePlayer(string connectionId) {
            var removed = Players.RemoveAll(p => p.ConnectionId == connectionId) > 0;
            return removed;
        }

        public bool SetPlayerReady(string connectionId, bool isReady) {
            for (int i = 0; i < Players.Count; i++) {
                if (Players[i].ConnectionId == connectionId) {
                    var player = Players[i];
                    player.IsReady = isReady;
                    Players[i] = player;
                    return true;
                }
            }
            return false;
        }
        public bool AreAllPlayersReady() {
            if (Players.Count < 2) return false;

            return Players.All(p => p.IsReady);
        }
        public bool StartGame(List<string> statementIds) {
            if (State == GameState.InProgress) return false;

            State = GameState.InProgress;
            ActiveStatementIds = statementIds;
            return true;
        }
        public void TransferHost(){
            if (Players.Any())
                Host = Players.First().Username;
        }
        public bool IsEmpty() => Players.Count == 0;
    }
