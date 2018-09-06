
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace BFYOC
{
    /*
    Request:
    {
        "userId": "cc20a6fb-a91f-4192-874d-132493685376",
        "productId": "4c25613a-a3c2-4ef3-8e02-9c335eb23204",
        "locationName": "Sample ice cream shop",
        "rating": 5,
        "userNotes": "I love the subtle notes of orange in this ice cream!"
    }

    Response:
    {
        "id": "79c2779e-dd2e-43e8-803d-ecbebed8972c",
        "userId": "cc20a6fb-a91f-4192-874d-132493685376",
        "productId": "4c25613a-a3c2-4ef3-8e02-9c335eb23204",
        "timestamp": "2018-05-21 21:27:47Z",
        "locationName": "Sample ice cream shop",
        "rating": 5,
        "userNotes": "I love the subtle notes of orange in this ice cream!"
    }
    */
    public static class CreateRating
    {
        private static readonly HttpClient client = new HttpClient();

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ratings")]HttpRequest req,
            [CosmosDB(
                databaseName: "Challenge2",
                collectionName: "Ratings",
                ConnectionStringSetting = "CosmosDBConnectionString")]
                IAsyncCollector<Rating> document,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");

                // Grab data from body
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                // Validate userId
                var response = await client.GetAsync($"https://serverlessohlondonuser.azurewebsites.net/api/GetUser?userId={data.userId}");
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return (ActionResult)new BadRequestObjectResult($"UserId: {data.userId} doesn't exists");
                }

                // Validate productId
                response = await client.GetAsync($"https://serverlessohlondonproduct.azurewebsites.net/api/GetProduct?productId={data.productId}");
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return (ActionResult)new BadRequestObjectResult($"ProductId: {data.productId} doesn't exists");
                }

                // Check rating is between 0 and 5
                int ratingValue = -1;
                if (!int.TryParse((string)data.rating, out ratingValue) || ratingValue < 0 || ratingValue > 5)
                {
                    return (ActionResult)new BadRequestObjectResult("Rating needs to be between 0 and 5");
                }

                var andysCoolService = new CosmosService();

                var rating = await andysCoolService.CreateRatingFromDocument(data, document, client, log);

                return (ActionResult)new OkObjectResult(rating);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return (ActionResult)new BadRequestObjectResult("Error");
            }
        }
    }
}
