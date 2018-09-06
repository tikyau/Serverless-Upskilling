using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BFYOC
{
    public class CosmosService
    {
        public async Task<Rating> CreateRatingFromDocument(dynamic data, IAsyncCollector<Rating> document, HttpClient client, ILogger logger)
        {
            // Set unique id
            var id = Guid.NewGuid();

            var postData = new
            {
                documents = new[]
                    {
                        new
                        {
                            language = "en",
                            id = id,
                            text = data.userNotes
                        }
                    }
            };



            //CognitivesServicesApiKey
            client.DefaultRequestHeaders.Clear();
            var key = Environment.GetEnvironmentVariable("CognitivesServicesApiKey");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            var sentimentResponse = await client.PostAsJsonAsync("https://westeurope.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment", postData);

            double sentimentScore = 0;

            if (sentimentResponse.IsSuccessStatusCode)
            {
                var scores = await sentimentResponse.Content.ReadAsAsync<dynamic>();


                var documentScore = ((IEnumerable<dynamic>)scores.documents).SingleOrDefault();
                sentimentScore = double.Parse((string)documentScore.score);
            }

            logger.LogMetric("SentimentMetric", sentimentScore);

            // Set Timestamp
            var timestamp = DateTime.UtcNow;

            var rand = new Random();

            var rating = new Rating
            {
                id = id,
                userId = data.userId,
                productId = data.productId,
                timestamp = timestamp,
                locationName = data.locationName,
                rating = data.rating,
                userNotes = data.userNotes,
                magicNumber = rand.Next(),
                sentimentScore = sentimentScore
            };

            await document.AddAsync(rating);

            return rating;
        }
    }
}