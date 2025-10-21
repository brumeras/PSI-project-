using KNOTS.Data;
using KNOTS.Models;

namespace KNOTS.Services;

public class StatementRepository
{
    private readonly AppDbContext _context;

    public StatementRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<GameStatement> GetAllStatements()
    {
        return _context.Statements.ToList();
    }

    public void EnsureDefaultStatements()
    {
        if (!_context.Statements.Any())
        {
            var defaults = new List<GameStatement>
            {
                new() { Id = "S1", Text = "I like getting up early in the morning" },
                new() { Id = "S2", Text = "I prefer relaxing at home over going to parties" },
                new() { Id = "S3", Text = "I enjoy spontaneous trips" },
                // ... continue with your default 20
            };

            _context.Statements.AddRange(defaults);
            _context.SaveChanges();
        }
    }
}
