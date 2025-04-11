using System;
using dtos_service_insights_tests.TestServices;
using Reqnroll;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using dtos_service_insights_tests.Config;
using dtos_service_insights_tests.Contexts;
using dtos_service_insights_tests.Models;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace dtos_service_insights_tests.StepDefinitions;

[Binding]
public sealed class FileUploadSteps
{
    private readonly EndToEndFileUploadService _fileUploadService;
    private readonly AppSettings _appSettings;
    private SmokeTestsContexts _smokeTestsContext;

    public FileUploadSteps(IServiceProvider services, AppSettings appSettings, SmokeTestsContexts smokeTestsContext)
    {
        _appSettings = appSettings;
        _smokeTestsContext = smokeTestsContext;
        _fileUploadService = services.GetRequiredService<EndToEndFileUploadService>();
    }

    [Given(@"the database is cleaned of all records for Episode Ids: (.*)")]
    public async Task GivenDatabaseIsCleaned(string episodeIdsString)
    {
        var episodeIds = episodeIdsString.Split(',', StringSplitOptions.TrimEntries);

        // _fileUploadService.CleanDatabaseAsync accepts a list of NHS numbers
        await _fileUploadService.CleanDatabaseAsync(episodeIds);
    }

    [Given(@"the application is properly configured")]
    public void GivenApplicationIsConfigured()
    {
        _fileUploadService.Should().NotBeNull("EndToEndFileUploadService is not initialized.");
    }

    [Given(@"file (.*) exists in the configured location for ""(.*)"" with Episode Ids : (.*)")]
    public void GivenFileExistsAtConfiguredPath(string fileName, string? recordType, string episodeIdsData)
    {
        string workingDirectory = Environment.CurrentDirectory;
        string path = Directory.GetParent(workingDirectory).Parent.Parent.Parent.Parent.FullName;

        var folderPath=path + "/TestData/" + recordType +"/";
            var filePath = Path.Combine(folderPath!, fileName);


            _smokeTestsContext.FilePath = filePath;
            _smokeTestsContext.RecordType = (RecordTypesEnum)Enum.Parse(typeof(RecordTypesEnum), recordType, ignoreCase: true);

            _smokeTestsContext.EpisodeIds = episodeIdsData.Split(',', StringSplitOptions.TrimEntries).ToList();
    }

    [Given(@"the file is uploaded to the Blob Storage container")]
    [When(@"the file is uploaded to the Blob Storage container")]
    public async Task WhenFileIsUploaded()
    {
        var filePath =_smokeTestsContext.FilePath;
        await _fileUploadService.UploadFileAsync(filePath);
    }

    [Given(@"the Episode Ids in the database should match the file data")]
    [Then(@"the Episode Ids in the database should match the file data")]
    public async Task ThenVerifyEpisodeIdsInDatabase()
    {
        await _fileUploadService.VerifyEpisodeIdsAsync("EPISODE", _smokeTestsContext.EpisodeIds!);
    }

    [Then(@"the matching episode data from csv is inserted into DB")]
    [Then(@"latest changes to the episode are loaded into the Episode Manager")]
    public async Task ThenTheMatchingEpisodeDataFromCsvIsInsertedIntoDB()
    {
        await _fileUploadService.VerifyFullDatabaseRecordAsync("EPISODE",_smokeTestsContext.EpisodeIds.FirstOrDefault(),_smokeTestsContext.FilePath);
    }

    [Then("there should be {int} records for the Episode Id {string} in the database")]
    public async Task ThenThereShouldBeRecordsForTheEpisodeIdInTheDatabase(int count, string episodeId)
    {
        await _fileUploadService.VerifyEpisodeIdsCountAsync("EPISODE", episodeId,count);
    }

    [Then("the database should match the amended (.*) for the Episode Id")]
    public async Task ThenTheDatabaseShouldMatchTheAmendedForTheEpisodeId(string expectedGivenName)
    {
        await _fileUploadService.VerifyFieldUpdateAsync("EPISODE", _smokeTestsContext.EpisodeIds.FirstOrDefault(), "EPISODE_OPEN_DATE", expectedGivenName);
    }

    [Then("the episode data from file should be inserted or updated in the database")]
    public async Task ThenTheEpisodeDataFromFileShouldBeInsertedOrUpdatedInTheDatabase()
    {
        await _fileUploadService.VerifyFullDatabaseRecordAsync("EPISODE",_smokeTestsContext.EpisodeIds.FirstOrDefault(),_smokeTestsContext.FilePath);
    }
}
