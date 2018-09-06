using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Azure.EventHubs;

namespace BFYOC
{
    public static class SalesPlacedEventHandler
    {

        [FunctionName("SalesPlacedEventHandler")]
        public static async Task Run(
            [EventHubTrigger("32inside", Connection = "EventHubConnectionAppSetting")]string[] messages,
            [CosmosDB(
                databaseName: "Challenge2",
                collectionName: "sales",
                ConnectionStringSetting = "CosmosDBConnectionString")]
                IAsyncCollector<dynamic> document,
            TraceWriter log)
        {
            foreach (var message in messages)
            {
                await document.AddAsync(message);
            }
        }
    }
}
