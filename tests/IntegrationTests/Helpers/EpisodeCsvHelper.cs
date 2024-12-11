using System.Collections.Generic;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace IntegrationTests.Helpers
{
    public static class EpisodeCsvHelper
    {
        public static List<string> ExtractEpisodeIds(string filePath)
        {
            var episodeIds = new List<string>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    string episodeId = csv.GetField<string>("episode_id");
                    episodeIds.Add(episodeId);
                }
            }

            return episodeIds;
        }
    }
}
