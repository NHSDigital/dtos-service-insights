using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using Ddtos_service_insights_tests.Helpers;
using Microsoft.Extensions.Logging;
using Azure;
using AllOverIt.Extensions;
using System.Collections;

namespace dtos_service_insights_tests.Helpers;

public class ApiClientHelper
{
    private static readonly Dictionary<string, string> CsvApiFieldMap = new Dictionary<string, string>
{
        {"nhs_number","NhsNumber"},
        {"episode_id","EpisodeId"},
        {"episode_date","EpisodeOpenDate"},
        {"date_of_foa","FirstOfferedAppointmentDate"},
        {"date_of_as","ActualScreeningDate"},
        {"early_recall_date","EarlyRecallDate"},
        {"call_recall_status_authorised_by","CallRecallStatusAuthorisedBy"},
        {"bso_batch_id","BatchId"},
        {"reason_closed_code","ReasonClosedCode"},
        {"end_point","EndPoint"}
};

    public async Task<RestResponse> GetApiResponseAsync(string endPoint)
        {
            RestClient restClient = new RestClient();
            RestRequest request = new RestRequest(endPoint)
            {
                Method = Method.Get
            };
            request.AddParameter("page", 1);
            request.AddParameter("pageSize", 4);
            request.AddParameter("startDate", "2020-01-01");
            request.AddParameter("endDate", "2025-01-01");
            request.AddHeaders(new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Ocp-Apim-Subscription-Key", "d8d079c53ba34c4e82dc66d86bba5b93" }
            });
            RestResponse response = await restClient.ExecuteAsync(request);
            return await restClient.ExecuteAsync(request);
        }

    public bool VerifyCsvWithApiResponseAsync(RestResponse restResponse, string episodeId, string csvFilePath, ILogger logger)
    {
        var csvRecords = CsvHelperService.ReadCsv(csvFilePath);
        var expectedRecord = csvRecords.FirstOrDefault(record => record["episode_id"] == episodeId);
        if (expectedRecord == null)
        {
            logger.LogError($"Episode ID {episodeId} not found in the CSV file.");
            return false;
        }
        JObject jsonObject=JObject.Parse(restResponse.Content);
        JArray episodeArray = (JArray)jsonObject["episodes"]!;
        for (int i=0; i < episodeArray.Count; i ++)
        {

            if(episodeArray[i]["EpisodeId"].ToString().Equals(episodeId))
            {
                foreach( KeyValuePair<string, string> kvp in CsvApiFieldMap )
                {
                            var expectedValue = expectedRecord[kvp.Key.ToString()];
                            var actualValue = episodeArray[0][kvp.Value.ToString()];
                            if(kvp.Value.ToString().Contains("Date") && !actualValue.IsNullOrEmpty())
                            {
                            if ((DateTime)actualValue is DateTime dateTimeValue)
                            {
                                DateTime date = System.Convert.ToDateTime(actualValue);
                                actualValue=date.ToString("yyyy-MM-dd");
                            }
                            }

                            if(!String.IsNullOrEmpty(expectedValue) )
                            {
                            if (expectedValue != actualValue.ToString())
                            {
                                logger.LogError($"Inside Mismatch in {kvp.Key.ToString()} for Episode Id {episodeId}: expected '{expectedValue}', found '{actualValue}'.");
                                return false;
                            }
                            }
                            else if ( !String.IsNullOrEmpty(actualValue.ToString()))
                            {
                                logger.LogError($"Mismatch in {kvp.Key.ToString()} for Episode Id {episodeId}: expected '{expectedValue}', found '{actualValue}'.");
                                return false;
                            }


                }

            }
        }
        return true;
    }

    public bool VerifyEpisodeRecordCountInAPIResponse(RestResponse restResponse, string episodeId,string csvFilePath, ILogger logger, int expectedCount)
    {
        int indexMatchCount=0;
        JObject jsonObject=JObject.Parse(restResponse.Content);
        JArray episodeArray = (JArray)jsonObject["episodes"]!;
        for (int i=0; i < episodeArray.Count; i ++)
        {

          if(episodeArray[i]["EpisodeId"].ToString()==episodeId)
          {
            indexMatchCount++;

          }
        }
        if( expectedCount == indexMatchCount )
        return true;

      return false;

    }

    public bool VerifyParticipantsRecordCountInAPIResponse(RestResponse restResponse, string episodeId,string csvFilePath, ILogger logger, int expectedCount)
    {
        var csvRecords = CsvHelperService.ReadCsv(csvFilePath);
        var expectedRecord = csvRecords.FirstOrDefault(record => record["episode_id"] == episodeId);
        int indexMatchCount=0;
        JObject jsonObject=JObject.Parse(restResponse.Content);
        JArray nhsNumberArray = (JArray)jsonObject["Profiles"]!;
        for (int i=0; i < nhsNumberArray.Count; i ++)
        {

          if(nhsNumberArray[i]["NhsNumber"].ToString()==expectedRecord["nhs_number"])
          {
            indexMatchCount++;

          }
        }
        if( expectedCount == indexMatchCount )
        return true;

      return false;

    }

}

