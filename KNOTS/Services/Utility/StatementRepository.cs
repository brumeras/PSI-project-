using System.Collections.Generic;
using System.Linq;
using KNOTS.Data;
using KNOTS.Models;

namespace KNOTS.Services;

/// <summary>
/// Provides data access and management for <see cref="GameStatement"/> entities.
/// Handles retrieval, initialization of default statements, and persistence using the database context.
/// </summary>
public class StatementRepository{
    private readonly AppDbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StatementRepository"/> class.
    /// </summary>
    /// <param name="context">The application database context used to manage statement data.</param>
    /// <remarks>
    /// When created, this repository connects to the <see cref="AppDbContext"/> instance for database operations.
    /// </remarks>
    public StatementRepository(AppDbContext context) { _context = context; }
    
    /// <summary>
    /// Retrieves all available <see cref="GameStatement"/> records from the database.
    /// </summary>
    /// <returns>A list containing all statements currently stored in the database.</returns>
    public List<GameStatement> GetAllStatements() {return _context.Statements.ToList(); }
    
    /// <summary>
    /// Ensures that the database contains a set of default statements.
    /// </summary>
    /// <remarks>
    /// If the statements table is empty, this method seeds the database with a predefined list of
    /// default <see cref="GameStatement"/> entries and saves them immediately.
    /// </remarks>
    /// <example>
    /// <code>
    /// var repo = new StatementRepository(context);
    /// repo.EnsureDefaultStatements();
    /// </code>
    /// </example>
    public void EnsureDefaultStatements() {
        if (!_context.Statements.Any()) {
            var defaults = new List<GameStatement> {
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
