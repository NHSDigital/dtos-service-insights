using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NHS.ServiceInsights.Data;

namespace IntegrationTests.Helpers
{
    public class DatabaseHelper
    {
        private readonly ServiceInsightsDbContext _dbContext;
        private readonly ILogger<DatabaseHelper> _logger;

        public DatabaseHelper(ServiceInsightsDbContext dbContext, ILogger<DatabaseHelper> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task CleanDatabaseAsync()
        {
            // Remove all existing episodes before starting test
            _dbContext.Episodes.RemoveRange(_dbContext.Episodes);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Database cleanup completed.");
        }

        public async Task<bool> EpisodeExistsAsync(string episodeId)
        {
            // Check if the episode exists in the database
            var exists = await _dbContext.Episodes.AnyAsync(e => e.EpisodeId == episodeId);
            return exists;
        }
    }
}
