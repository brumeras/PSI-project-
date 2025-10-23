using System.Collections.Concurrent;

namespace KNOTS.Services;

/// <summary>
/// Repository responsible for storing and retrieving player swipes for game rooms.
/// Provides thread-safe operations and persists data to a JSON file.
/// </summary>
public class SwipeRepository {
        private readonly ConcurrentDictionary<string, List<PlayerSwipe>> _roomSwipes = new();
        private readonly JsonFileRepository<Dictionary<string, List<PlayerSwipe>>> _fileRepository;
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// Initializes a new instance of <see cref="SwipeRepository"/> with the specified file repository.
        /// </summary>
        /// <param name="fileRepository">The JSON file repository used for persistence.</param>
        public SwipeRepository(JsonFileRepository<Dictionary<string, List<PlayerSwipe>>> fileRepository) {
            _fileRepository = fileRepository;
            LoadSwipes();
        }
        
        /// <summary>
        /// Loads swipes from the underlying file repository into memory.
        /// Thread-safe.
        /// </summary>
        private void LoadSwipes() {
            lock (_lockObject) {
                var data = _fileRepository.Load();
                _roomSwipes.Clear();
                foreach (var kvp in data) { _roomSwipes[kvp.Key] = kvp.Value; }
            }
        }
        
        /// <summary>
        /// Saves a swipe for a player in a specific room.
        /// Replaces any previous swipe for the same statement by the same player.
        /// Persists the updated data to the JSON file.
        /// Thread-safe.
        /// </summary>
        /// <param name="roomCode">The code of the room.</param>
        /// <param name="swipe">The player swipe to save.</param>
        /// <returns>True if the swipe was successfully saved.</returns>
        public bool SaveSwipe(string roomCode, PlayerSwipe swipe) {
            lock (_lockObject) {
                LoadSwipesInternal();
                if (!_roomSwipes.ContainsKey(roomCode)) { _roomSwipes[roomCode] = new List<PlayerSwipe>(); }

                _roomSwipes[roomCode].RemoveAll(s => 
                    s.PlayerUsername == swipe.PlayerUsername && s.StatementId == swipe.StatementId);

                _roomSwipes[roomCode].Add(swipe);
                
                PersistSwipes();
                
                Console.WriteLine($"[SwipeRepository] Saved swipe for {swipe.PlayerUsername} in room {roomCode}. Total swipes in room: {_roomSwipes[roomCode].Count}");
                return true;
            }
        }
        
        /// <summary>
        /// Gets all swipes for a specific room.
        /// Thread-safe.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <returns>List of <see cref="PlayerSwipe"/> for the room.</returns>
        public List<PlayerSwipe> GetRoomSwipes(string roomCode) {
            lock (_lockObject) {
                // Reload to get fresh data
                LoadSwipesInternal();
                
                _roomSwipes.TryGetValue(roomCode, out var swipes);
                var result = swipes ?? new List<PlayerSwipe>();
                
                Console.WriteLine($"[SwipeRepository] GetRoomSwipes for {roomCode}: Found {result.Count} swipes");
                return result;
            }
        }
        
        /// <summary>
        /// Gets all swipes for a specific player in a specific room.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        /// <param name="playerUsername">The player's username.</param>
        /// <returns>List of <see cref="PlayerSwipe"/> for the player.</returns>
        public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) {
            var roomSwipes = GetRoomSwipes(roomCode);
            var playerSwipes = roomSwipes.Where(s => s.PlayerUsername == playerUsername).ToList();
            Console.WriteLine($"[SwipeRepository] GetPlayerSwipes for {playerUsername} in room {roomCode}: Found {playerSwipes.Count} swipes");
            return playerSwipes;
        }
        
        
        /// <summary>
        /// Clears all swipes for a given room and persists the change.
        /// Thread-safe.
        /// </summary>
        /// <param name="roomCode">The room code.</param>
        public void ClearRoomData(string roomCode) {
            lock (_lockObject) {
                _roomSwipes.TryRemove(roomCode, out _);
                PersistSwipes();
            }
        }
        
        /// <summary>
        /// Loads swipes from the file repository into memory without locking.
        /// Used internally to refresh data before operations.
        /// </summary>
        private void LoadSwipesInternal() {
            var data = _fileRepository.Load();
            _roomSwipes.Clear();
            foreach (var kvp in data) { _roomSwipes[kvp.Key] = kvp.Value; }
        }
        
        /// <summary>
        /// Persists the in-memory swipe data to the JSON file repository.
        /// </summary>
        private void PersistSwipes() {
            var data = _roomSwipes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _fileRepository.Save(data);
        }
    }