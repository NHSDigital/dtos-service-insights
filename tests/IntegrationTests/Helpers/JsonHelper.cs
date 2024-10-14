using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IntegrationTests.Helpers
{
    public static class JsonHelper
    {
        public static List<string> ExtractEpisodeIds(string filePath)
        {
            var episodeIds = new List<string>();

            var jsonString = File.ReadAllText(filePath);
            using var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("Episodes", out JsonElement episodesElement) && episodesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var episode in episodesElement.EnumerateArray())
                {
                    if (episode.TryGetProperty("episode_id", out JsonElement idElement))
                    {
                        var episodeId = idElement.GetString();
                        if (!string.IsNullOrEmpty(episodeId))
                        {
                            episodeIds.Add(episodeId);
                        }
                    }
                }
            }

            return episodeIds;
        }
    }
}

