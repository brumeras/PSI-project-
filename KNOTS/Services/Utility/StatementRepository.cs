namespace KNOTS.Services;

public class StatementRepository {
        private readonly JsonFileRepository<List<GameStatement>> _fileRepository;
        private List<GameStatement> _statements;

        public StatementRepository(JsonFileRepository<List<GameStatement>> fileRepository) {
            _fileRepository = fileRepository;
            _statements = _fileRepository.Load();
            
            if (!_statements.Any()) {
                _statements = CreateDefaultStatements();
                _fileRepository.Save(_statements);
            }
        }

        public List<GameStatement> GetAll() => _statements;
        public List<GameStatement> GetRandom(int count) {
            var random = new Random();
            return _statements.OrderBy(x => random.Next()).Take(Math.Min(count, _statements.Count)).ToList();
        }

        public GameStatement? GetById(string id) {
            var statement = _statements.FirstOrDefault(s => s.Id == id);
            return statement.Id != null ? statement : null;
        }

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