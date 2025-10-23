namespace KNOTS.Services;

/// <summary>
/// Repository for managing game statements. Handles loading, saving, and providing default statements.
/// </summary>
public class StatementRepository {
        private readonly JsonFileRepository<List<GameStatement>> _fileRepository;
        private List<GameStatement> _statements;

        
        /// <summary>
        /// Initializes a new instance of <see cref="StatementRepository"/> and loads statements from storage.
        /// If no statements are found, default statements are created and saved.
        /// </summary>
        /// <param name="fileRepository">The JSON file repository to load and save statements.</param>
        public StatementRepository(JsonFileRepository<List<GameStatement>> fileRepository) {
            _fileRepository = fileRepository;
            _statements = _fileRepository.Load();
            
            if (!_statements.Any()) {
                _statements = CreateDefaultStatements();
                _fileRepository.Save(_statements);
            }
        }

        
        /// <summary>
        /// Returns all loaded statements.
        /// </summary>
        /// <returns>List of all <see cref="GameStatement"/> objects.</returns>
        public List<GameStatement> GetAll() => _statements;
        
        
        /// <summary>
        /// Returns a randomized subset of statements.
        /// </summary>
        /// <param name="count">The number of statements to retrieve.</param>
        /// <returns>Random list of <see cref="GameStatement"/> objects.</returns>
        public List<GameStatement> GetRandom(int count) {
            var random = new Random();
            return _statements.OrderBy(x => random.Next()).Take(Math.Min(count, _statements.Count)).ToList();
        }

        /// <summary>
        /// Retrieves a statement by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the statement.</param>
        /// <returns>The <see cref="GameStatement"/> if found; otherwise, null.</returns>
        public GameStatement? GetById(string id) {
            var statement = _statements.FirstOrDefault(s => s.Id == id);
            return statement.Id != null ? statement : null;
        }

        
        /// <summary>
        /// Creates a default list of statements to use if none exist in storage.
        /// </summary>
        /// <returns>List of default <see cref="GameStatement"/> objects.</returns>
        private List<GameStatement> CreateDefaultStatements() {
            return new List<GameStatement>
            {
                new GameStatement("S1", "I like getting up early in the morning"),
                new GameStatement("S2", "I prefer relaxing at home over going to parties"),
                new GameStatement("S3", "I enjoy spontaneous trips"),
                new GameStatement("S4", "Animals are an important part of my life"),
                new GameStatement("S5", "I prefer movies over theater"),
                new GameStatement("S6", "Sports are part of my daily routine"),
                new GameStatement("S7", "I enjoy cooking at home"),
                new GameStatement("S8", "Summer is the best season"),
                new GameStatement("S9", "Meaningful conversations matter more to me than having fun"),
                new GameStatement("S10", "I like taking risks and trying new things"),
                new GameStatement("S11", "Music is an important part of my life"),
                new GameStatement("S12", "I value personal space in relationships"),
                new GameStatement("S13", "I like to plan everything in advance"),
                new GameStatement("S14", "I feel good at large parties"),
                new GameStatement("S15", "I live in the moment and don't worry about the future"),
                new GameStatement("S16", "Romantic relationships are important to me"),
                new GameStatement("S17", "I like video games"),
                new GameStatement("S18", "Books are better than movies"),
                new GameStatement("S19", "I enjoy nature and hiking"),
                new GameStatement("S20", "Financial stability is a priority"),
            };
        }
    }