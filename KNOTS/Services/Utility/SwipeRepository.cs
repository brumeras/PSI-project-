using System.Collections.Concurrent;

namespace KNOTS.Services;

public class SwipeRepository {
        private readonly ConcurrentDictionary<string, List<PlayerSwipe>> _roomSwipes = new();
        private readonly JsonFileRepository<Dictionary<string, List<PlayerSwipe>>> _fileRepository;
        private readonly object _lockObject = new object();
        public SwipeRepository(JsonFileRepository<Dictionary<string, List<PlayerSwipe>>> fileRepository) {
            _fileRepository = fileRepository;
            LoadSwipes();
        }
        private void LoadSwipes() {
            lock (_lockObject) {
                var data = _fileRepository.Load();
                _roomSwipes.Clear();
                foreach (var kvp in data) { _roomSwipes[kvp.Key] = kvp.Value; }
            }
        }
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
        public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) {
            var roomSwipes = GetRoomSwipes(roomCode);
            var playerSwipes = roomSwipes.Where(s => s.PlayerUsername == playerUsername).ToList();
            Console.WriteLine($"[SwipeRepository] GetPlayerSwipes for {playerUsername} in room {roomCode}: Found {playerSwipes.Count} swipes");
            return playerSwipes;
        }
        public void ClearRoomData(string roomCode) {
            lock (_lockObject) {
                _roomSwipes.TryRemove(roomCode, out _);
                PersistSwipes();
            }
        }
        private void LoadSwipesInternal() {
            var data = _fileRepository.Load();
            _roomSwipes.Clear();
            foreach (var kvp in data) { _roomSwipes[kvp.Key] = kvp.Value; }
        }
        private void PersistSwipes() {
            var data = _roomSwipes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _fileRepository.Save(data);
        }
    }