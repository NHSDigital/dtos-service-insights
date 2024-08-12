using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace GetParticipant
{
    public static class GetParticipant
    {
        [FunctionName("GetParticipant")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Request to retrieve a participant has been processed.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(requestBody);
            string NhsNumber = data?.nhs_number;

            if (string.IsNullOrEmpty(NhsNumber))
            {
                return new BadRequestObjectResult("Please enter a valid NHS Number.");
            }

            var participant = ParticipantRepository.GetParticipantByNhsNumber(NhsNumber);

            if (participant == null)
            {
                return new NotFoundObjectResult($"Participant with ID {NhsNumber} not found.");
            }

            return new OkObjectResult(participant);
        }

    }
}
