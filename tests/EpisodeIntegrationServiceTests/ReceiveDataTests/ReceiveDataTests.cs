using Moq;
using Microsoft.Extensions.Logging;
using NHS.ServiceInsights.Common;
using System.Text;

namespace NHS.ServiceInsights.EpisodeManagementService;

[TestClass]
public class ReceiveDataTests
{
    private readonly Mock<IHttpRequestService> _mockHttpRequestService = new();
    private readonly Mock<ILogger<ReceiveData>> _mockLogger = new();
    private readonly ReceiveData _function;
    public ReceiveDataTests()
    {
        Environment.SetEnvironmentVariable("ProcessDataURL", "ProcessDataURL");

        _function = new ReceiveData(_mockLogger.Object, _mockHttpRequestService.Object);
    }

    [TestMethod]
    public async Task Run_ShouldLogValidJsonAndCallSendPost()
    {
        // Arrange
        string data = "\"nhs_number\",\"episode_id\",\"episode_type\",\"change_db_date_time\",\"episode_date\",\"appointment_made\",\"date_of_foa\",\"date_of_as\",\"early_recall_date\",\"call_recall_status_authorised_by\",\"end_code\",\"end_code_last_updated\",\"bso_organisation_code\",\"bso_batch_id\",\"reason_closed_code\",\"end_point\",\"final_action_code\"\n" +
        "\"9999999999\",1000,\"C\",\"2022-08-17 13:02:17.110314+01\",\"2022-08-17\",,,,,,,,\"AGA\",\"AGA000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-09-02 14:30:54.121779+01\",\"2022-09-02\",,,,,,,,\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999998\",2000,\"C\",\"2022-10-13 22:52:34.825602+01\",\"2022-09-02\",\"True\",\"2022-09-27\",,,\"SCREENING_OFFICE\",\"DNA\",\"2022-10-13 00:00:00+01\",\"ANE\",\"ANE000000A\",,,\n" +
        "\"9999999999\",1000,\"C\",\"2022-11-08 22:32:23.326676+00\",\"2022-08-17\",\"True\",\"2022-09-18\",\"2022-11-05\",,\"SCREENING_OFFICE\",\"SC\",\"2022-11-08 00:00:00+00\",\"AGA\",\"AGA000000A\",,,";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        // Act
        await _function.Run(stream, "testdata.csv");

        // Assert
        _mockLogger.Verify(log =>
            log.Log(
            LogLevel.Information,
            0,
            It.Is<object>(state => state.ToString().Contains("Sending CSV data to the ProcessData function")),
            null,
            (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);

        _mockHttpRequestService.Verify(x => x.SendPost("ProcessDataURL?FileName=testdata.csv", It.IsAny<string>()), Times.Once);
    }
}



