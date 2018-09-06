
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BFYOC
{
    public static class GetRatingsForUser
    {
        [FunctionName("GetRatingsForUser")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{userId:guid}/ratings")]HttpRequest req, 
            [CosmosDB("Challenge2", "Ratings",
                ConnectionStringSetting = "CosmosDBConnectionString",
                SqlQuery = "select * from Ratings r where r.userId = {userId}")]
                IEnumerable<Rating> ratings,
            TraceWriter log
        )
        {
            return new OkObjectResult(ratings);
        }
    }
}
