/*using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using KNOTS.Data;
using KNOTS.Models;

namespace KNOTS.Services;

public class SwipeRepository {
    private readonly AppDbContext _context;
    public SwipeRepository(AppDbContext context) { _context = context; }
    public bool SaveSwipe(string roomCode, string playerUsername, string statementId, bool agree) {
        var statement = _context.Statements.FirstOrDefault(s => s.Id == statementId);
        if (statement == null) return false;

        var existing = _context.PlayerSwipes
            .FirstOrDefault(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername && s.StatementId == statementId);

        if (existing != null)
            _context.PlayerSwipes.Remove(existing);

        _context.PlayerSwipes.Add(new PlayerSwipeRecord {
            RoomCode = roomCode,
            PlayerUsername = playerUsername,
            StatementId = statementId,
            StatementText = statement.Text,
            AgreeWithStatement = agree,
            SwipedAt = DateTime.Now
        });

        _context.SaveChanges();
        return true;
    }
    public List<PlayerSwipe> GetPlayerSwipes(string roomCode, string playerUsername) {
        return _context.PlayerSwipes
            .Where(s => s.RoomCode == roomCode && s.PlayerUsername == playerUsername)
            .Select(s => new PlayerSwipe(s.PlayerUsername, s.StatementId, s.StatementText, s.AgreeWithStatement)
                { SwipedAt = s.SwipedAt })
            .ToList();
    }
    public List<PlayerSwipe> GetRoomSwipes(string roomCode) {
        return _context.PlayerSwipes
            .Where(s => s.RoomCode == roomCode)
            .Select(s => new PlayerSwipe(s.PlayerUsername, s.StatementId, s.StatementText, s.AgreeWithStatement)
                { SwipedAt = s.SwipedAt })
            .ToList();
    }
    public void ClearRoomData(string roomCode) {
        var swipes = _context.PlayerSwipes.Where(s => s.RoomCode == roomCode);
        _context.PlayerSwipes.RemoveRange(swipes);
        _context.SaveChanges();
    }
}
*/