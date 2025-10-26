using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; //players ir results bus savinami kaip json strings
using KNOTS.Data;
using KNOTS.Models;
using KNOTS.Services.Compability;
using KNOTS.Compability;
using Microsoft.EntityFrameworkCore;

namespace KNOTS.Services
{
    public class GameHistoryService
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;
        private readonly CompatibilityCalculator _calculator;

        public GameHistoryService(AppDbContext context, UserService userService, CompatibilityCalculator calculator)
        {
            _context = context;
            _userService = userService;
            _calculator = calculator;
        }

        public void SaveGame(string roomCode, List<string> players)
        {
            if (players == null || players.Count < 2)
                return;

            var results = new List<CompatibilityScore>();

            for (int i = 0; i < players.Count; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    results.Add(_calculator.Calculate(roomCode, players[i], players[j]));
                }
            }

            if (!results.Any())
                return;

            var best = results.OrderByDescending(r => r.Percentage).First();

            var history = new GameHistoryRecord
            {
                RoomCode = roomCode,
                PlayedDate = DateTime.Now,
                TotalPlayers = players.Count,
                PlayerUsernames = JsonSerializer.Serialize(players),
                BestMatchPlayer = best.Player2,
                BestMatchPercentage = best.Percentage,
                ResultsJson = JsonSerializer.Serialize(results)
            };

            _context.GameHistory.Add(history);
            _context.SaveChanges();

            // Update user stats
            foreach (var r in results)
            {
                _userService.UpdateUserStatistics(r.Player1, r.Percentage, false);
                _userService.UpdateUserStatistics(r.Player2, r.Percentage, false);
            }
        }

        public List<GameHistoryRecord> GetPlayerHistory(string playerUsername)
        {
            return _context.GameHistory
                .AsNoTracking()
                .Where(h => EF.Functions.Like(h.PlayerUsernames, $"%{playerUsername}%"))
                .OrderByDescending(h => h.PlayedDate)
                .ToList();
        }

        public List<GameHistoryRecord> GetAll()
        {
            return _context.GameHistory
                .AsNoTracking()
                .OrderByDescending(h => h.PlayedDate)
                .ToList();
        }
    }
}
