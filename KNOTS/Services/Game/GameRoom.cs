using System;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;

/// <summary>
/// Represents an active game room that manages players, game state, and session logic.
/// </summary>
/// <remarks>
/// The <see cref="GameRoom"/> class maintains information about the current host,
/// player list, and game progress. It also provides domain logic for joining,
/// readiness checks, and starting a game.
/// </remarks>
    public class GameRoom {
    
        /// <summary>
        /// Gets or sets the unique room code used to identify the game session.
        /// </summary>
        public string RoomCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the username of the player who is currently hosting the room.
        /// </summary>
        public string Host { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the list of players currently in the room.
        /// </summary>
        public List<GamePlayer> Players { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the timestamp indicating when the room was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the current state of the game room.
        /// </summary>
        public GameState State { get; set; } = GameState.WaitingForPlayers;
        
        /// <summary>
        /// Gets or sets the maximum number of players allowed in the room.
        /// </summary>
        public int MaxPlayers { get; set; } = 4;
        
        /// <summary>
        /// Gets or sets the list of active statement IDs for the current game session.
        /// </summary>
        public List<string> ActiveStatementIds { get; set; } = new();

        // Domain logic methods 
        
        /// <summary>
        /// Determines whether a player with the given username can join the room.
        /// </summary>
        /// <param name="username">The username of the player attempting to join.</param>
        /// <returns>
        /// A <see cref="JoinRoomResult"/> indicating whether the player can join and the current game state.
        /// </returns>
        public JoinRoomResult CanJoin(string username) {
            if (State != GameState.WaitingForPlayers) return new JoinRoomResult(false, "Game already in progress", State);
            if (Players.Count >= MaxPlayers) return new JoinRoomResult(false, "Room is full", State);
            if (Players.Any(p => p.Username == username)) return new JoinRoomResult(false, "Username already taken in this room", State);
            return new JoinRoomResult(true, "Can join", State);
        }
        
        /// <summary>
        /// Adds a player to the room and updates the room state if it becomes full.
        /// </summary>
        /// <param name="player">The player to add.</param>
        public void AddPlayer(GamePlayer player) {
            Players.Add(player);
            if (Players.Count >= MaxPlayers) State = GameState.InProgress;
        }
        
        /// <summary>
        /// Removes a player from the room based on their connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID of the player to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the player was removed; otherwise, <see langword="false"/>.
        /// </returns>
        public bool RemovePlayer(string connectionId) {
            var removed = Players.RemoveAll(p => p.ConnectionId == connectionId) > 0;
            return removed;
        }

        /// <summary>
        /// Updates a player's readiness status.
        /// </summary>
        /// <param name="connectionId">The connection ID of the player to update.</param>
        /// <param name="isReady">A value indicating whether the player is ready.</param>
        /// <returns>
        /// <see langword="true"/> if the player's readiness was successfully updated; otherwise, <see langword="false"/>.
        /// </returns>
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
        
        /// <summary>
        /// Determines whether all players are ready to start the game.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if at least two players exist and all are ready; otherwise, <see langword="false"/>.
        /// </returns>
        public bool AreAllPlayersReady() {
            if (Players.Count < 2) return false;
            return Players.All(p => p.IsReady);
        }
        
        /// <summary>
        /// Starts the game by setting the state to <see cref="GameState.InProgress"/> 
        /// and assigning the provided statements.
        /// </summary>
        /// <param name="statementIds">The list of statement IDs to activate for this game.</param>
        /// <returns>
        /// <see langword="true"/> if the game was started successfully; otherwise, <see langword="false"/>.
        /// </returns>
        public bool StartGame(List<string> statementIds) {
            if (State == GameState.InProgress) return false;
            State = GameState.InProgress;
            ActiveStatementIds = statementIds;
            return true;
        }
        
        /// <summary>
        /// Transfers host ownership to the first available player in the list.
        /// </summary>
        public void TransferHost(){
            if (Players.Any())
                Host = Players.First().Username;
        }
        /// <summary>
        /// Determines whether the room currently has no players.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if there are no players; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsEmpty() => Players.Count == 0;
    }
