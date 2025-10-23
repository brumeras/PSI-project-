using KNOTS.Compability;

namespace KNOTS.Services.Compability;

/// <summary>
/// Calculates compatibility scores between players based on their swipes.
/// </summary>
/// <remarks>
/// Compatibility is determined by comparing how players respond to the same statements.
/// </remarks>
public class CompatibilityCalculator {
        private readonly SwipeRepository _swipeRepository;
        
        public CompatibilityCalculator(SwipeRepository swipeRepository) { _swipeRepository = swipeRepository; }
        /// <summary>
        /// Calculates compatibility between two players in a specific room.
        /// </summary>
        public CompatibilityScore Calculate(string roomCode, string player1, string player2) {
            var player1Swipes = _swipeRepository.GetPlayerSwipes(roomCode, player1);
            var player2Swipes = _swipeRepository.GetPlayerSwipes(roomCode, player2);

            var matchedStatements = player1Swipes
                .Join(player2Swipes,
                    s1 => s1.StatementId,
                    s2 => s2.StatementId,
                    (s1, s2) => new { s1, s2 })
                .Where(pair => pair.s1.AgreeWithStatement == pair.s2.AgreeWithStatement)
                .Select(pair => pair.s1.StatementText)
                .ToList();
            
            int matchingSwipes = matchedStatements.Count;
            int totalStatements = Math.Min(player1Swipes.Count, player2Swipes.Count);

            return new CompatibilityScore(
                player1,
                player2,
                matchingSwipes,
                totalStatements,
                matchedStatements
            );
        }
        /// <summary>
        /// Calculates compatibility for all unique pairs of players in a room.
        /// </summary>
        public List<CompatibilityScore> CalculateAll(string roomCode, List<string> playerUsernames) {
            var results = new List<CompatibilityScore>();

            for (int i = 0; i < playerUsernames.Count; i++) {
                for (int j = i + 1; j < playerUsernames.Count; j++) {
                    var score = Calculate(roomCode, playerUsernames[i], playerUsernames[j]);
                    results.Add(score);
                }
            }
            return results.OrderByDescending(r => r.Percentage).ToList();
        }
        /// <summary>
        /// Finds the best match (highest compatibility) among all players.
        /// </summary>
        public CompatibilityScore? GetBestMatch(string roomCode, List<string> playerUsernames) {
            var allScores = CalculateAll(roomCode, playerUsernames);
            return allScores.FirstOrDefault();
        }
}