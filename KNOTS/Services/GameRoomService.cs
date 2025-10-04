using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services
{
    public class GameRoom
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public List<GamePlayer> Players { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsGameStarted { get; set; } = false;
        public int MaxPlayers { get; set; } = 4;
        public List<string> ActiveStatementIds { get; set; } = new(); // Kokie teiginiai naudojami šiame žaidime
    }

    // Pakeista į struct (value type)
    public struct GamePlayer
    {
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsReady { get; set; }

        public GamePlayer(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;
            JoinedAt = DateTime.Now;
            IsReady = false;
        }
    }

    // Pakeista į struct (value type)
    public struct JoinRoomResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public JoinRoomResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }

    // Pakeista į struct (value type)
    public struct DisconnectedPlayerInfo
    {
        public string Username { get; set; }
        public string RoomCode { get; set; }

        public DisconnectedPlayerInfo(string username, string roomCode)
        {
            Username = username;
            RoomCode = roomCode;
        }
    }

    public class GameRoomService
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
        private readonly ConcurrentDictionary<string, string> _playerToRoom = new(); // ConnectionId -> RoomCode
        private readonly ConcurrentDictionary<string, string> _connectionToUsername = new(); // ConnectionId -> Username
        private readonly Random _random = new();

        // Generuoja unikalų kambario kodą
        private string GenerateRoomCode()
        {
            string code;
            do
            {
                code = _random.Next(1000, 9999).ToString();
            } while (_rooms.ContainsKey(code));
            
            return code;
        }

        // Prideda žaidėją prie sistemos
        public void AddPlayer(string connectionId, string username)
        {
            _connectionToUsername[connectionId] = username;
        }

        // Sukuria naują kambarį
        public string CreateRoom(string hostConnectionId, string hostUsername)
        {
            var roomCode = GenerateRoomCode();
            
            var room = new GameRoom
            {
                RoomCode = roomCode,
                Host = hostUsername,
                Players = new List<GamePlayer>
                {
                    new GamePlayer(hostConnectionId, hostUsername) // Naudojamas konstruktorius
                }
            };

            _rooms[roomCode] = room;
            _playerToRoom[hostConnectionId] = roomCode;

            return roomCode;
        }

        // Prisijungia prie kambario
        public JoinRoomResult JoinRoom(string roomCode, string connectionId, string username)
        {
            if (!_rooms.TryGetValue(roomCode, out var room))
            {
                return new JoinRoomResult(false, "Room not found");
            }

            if (room.Players.Count >= room.MaxPlayers)
            {
                return new JoinRoomResult(false, "Room is full");
            }

            if (room.IsGameStarted)
            {
                return new JoinRoomResult(false, "Game has already started");
            }

            // Tikrina ar žaidėjas jau kambaryje
            if (room.Players.Any(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                return new JoinRoomResult(false, "A player with this username is already in game");
            }

            var player = new GamePlayer(connectionId, username);

            room.Players.Add(player);
            _playerToRoom[connectionId] = roomCode;
            _connectionToUsername[connectionId] = username;

            return new JoinRoomResult(true, "Successfully connected to a room!");
        }

        // Gauna kambario informaciją
        public GameRoom? GetRoomInfo(string roomCode)
        {
            _rooms.TryGetValue(roomCode, out var room);
            return room;
        }

        // Gauna visus kambario žaidėjų vardus
        public List<string> GetRoomPlayerUsernames(string roomCode)
        {
            if (_rooms.TryGetValue(roomCode, out var room))
            {
                return room.Players.Select(p => p.Username).ToList();
            }
            return new List<string>();
        }

        // Gauna žaidėjo vardą pagal ConnectionId
        public string GetPlayerUsername(string connectionId)
        {
            _connectionToUsername.TryGetValue(connectionId, out var username);
            return username ?? "";
        }

        // Pažymi žaidėją kaip ready
        public bool SetPlayerReady(string connectionId, bool isReady)
        {
            if (!_playerToRoom.TryGetValue(connectionId, out var roomCode))
            {
                return false;
            }

            if (!_rooms.TryGetValue(roomCode, out var room))
            {
                return false;
            }

            // Randame žaidėją ir atnaujiname jo ready būseną
            for (int i = 0; i < room.Players.Count; i++)
            {
                if (room.Players[i].ConnectionId == connectionId)
                {
                    var player = room.Players[i];
                    player.IsReady = isReady;
                    room.Players[i] = player; // Struct reikia priskirti atgal
                    return true;
                }
            }

            return false;
        }

        // Patikrina ar visi žaidėjai ready
        public bool AreAllPlayersReady(string roomCode)
        {
            if (!_rooms.TryGetValue(roomCode, out var room))
            {
                return false;
            }

            if (room.Players.Count < 2) // Reikia bent 2 žaidėjų
            {
                return false;
            }

            return room.Players.All(p => p.IsReady);
        }

        // Pradeda žaidimą kambaryje
        public bool StartGame(string roomCode, List<string> statementIds)
        {
            if (!_rooms.TryGetValue(roomCode, out var room))
            {
                return false;
            }

            if (room.IsGameStarted)
            {
                return false;
            }

            room.IsGameStarted = true;
            room.ActiveStatementIds = statementIds;
            return true;
        }

        // Gauna aktyvių teiginių ID
        public List<string> GetActiveStatementIds(string roomCode)
        {
            if (_rooms.TryGetValue(roomCode, out var room))
            {
                return room.ActiveStatementIds;
            }
            return new List<string>();
        }

        // Pašalina žaidėją iš sistemos
        public DisconnectedPlayerInfo RemovePlayer(string connectionId)
        {
            _connectionToUsername.TryGetValue(connectionId, out var username);
            var disconnectedUsername = username ?? "";
            var disconnectedRoomCode = "";

            if (_playerToRoom.TryRemove(connectionId, out var roomCode))
            {
                disconnectedRoomCode = roomCode;
                
                if (_rooms.TryGetValue(roomCode, out var room))
                {
                    room.Players.RemoveAll(p => p.ConnectionId == connectionId);
                    
                    // Jei kambarys tuščias, pašalinam jį
                    if (room.Players.Count == 0)
                    {
                        _rooms.TryRemove(roomCode, out _);
                    }
                    // Jei išėjo host'as, paskiriame naują
                    else if (room.Host == disconnectedUsername && room.Players.Count > 0)
                    {
                        room.Host = room.Players.First().Username;
                    }
                }
            }

            _connectionToUsername.TryRemove(connectionId, out _);
            return new DisconnectedPlayerInfo(disconnectedUsername, disconnectedRoomCode);
        }

        // Gauna visų kambarių sąrašą (debug tikslams)
        public List<GameRoom> GetAllRooms()
        {
            return _rooms.Values.ToList();
        }

        // Gauna kambario kodą pagal žaidėjo ConnectionId
        public string? GetPlayerRoomCode(string connectionId)
        {
            _playerToRoom.TryGetValue(connectionId, out var roomCode);
            return roomCode;
        }

        // Patikrina ar žaidėjas yra kambario host'as
        public bool IsPlayerHost(string connectionId)
        {
            if (!_playerToRoom.TryGetValue(connectionId, out var roomCode))
            {
                return false;
            }

            if (!_rooms.TryGetValue(roomCode, out var room))
            {
                return false;
            }

            var username = GetPlayerUsername(connectionId);
            return room.Host == username;
        }
    }
}