using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IntegrationTests.Helpers
{
    public class AzureFunctionRetryHelper
    {
        private readonly ILogger<AzureFunctionRetryHelper> _logger;

        public AzureFunctionRetryHelper(ILogger<AzureFunctionRetryHelper> logger)
        {
            _logger = logger;
        }

        public async Task<bool> RetryAsync(Func<Task<bool>> function, string itemDescription, int maxRetries = 10, int delayMilliseconds = 1000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                _logger.LogInformation("Attempt {Attempt} to retrieve {ItemDescription}", i + 1, itemDescription);

                try
                {
                    var result = await function();

                    if (result)
                    {
                        _logger.LogInformation("Successfully retrieved {ItemDescription} on attempt {Attempt}", itemDescription, i + 1);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Attempt {Attempt} failed with exception: {Exception}", i + 1, ex.Message);
                }

                _logger.LogInformation("{ItemDescription} not found, retrying in {DelayMilliseconds}ms...", itemDescription, delayMilliseconds);
                await Task.Delay(delayMilliseconds);
            }

            _logger.LogError("Failed to retrieve {ItemDescription} after {MaxRetries} retries.", itemDescription, maxRetries);
            return false;
        }
    }
}
