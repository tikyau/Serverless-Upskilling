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

namespace BFYOC
{
    /*
     {
	    "id" : "cb4be5ff-b01e-003b-4992-fd859d06d52d",
	    "topic" : "/subscriptions/c5eb460e-8650-4d67-b6b9-dd737f5fe2c0/resourceGroups/BFYOC/providers/Microsoft.Storage/storageAccounts/bfyocorders10",
	    "subject" : "/blobServices/default/containers/challengesixblob/blobs/20180606123200-ProductInformation.csv",
	    "data" : {
		    "api" : "PutBlob",
		    "clientRequestId" : "7ff4e16e-25f8-45b6-bcda-4c4106d6bdca",
		    "requestId" : "cb4be5ff-b01e-003b-4992-fd859d000000",
		    "eTag" : "0x8D5CBA98042E6F0",
		    "contentType" : "application/octet-stream",
		    "contentLength" : 264,
		    "blobType" : "BlockBlob",
		    "url" : "https://bfyocorders10.blob.core.windows.net/challengesixblob/20180606123200-ProductInformation.csv",
		    "sequencer" : "000000000000000000000000000036880000000000032d47",
		    "storageDiagnostics" : {
			    "batchId" : "d083296a-ae0a-49a2-9cab-c374d072a37f"
		    }
	    },
	    "eventType" : "Microsoft.Storage.BlobCreated",
	    "eventTime" : "2018-06-06T12:32:00.4582914Z",
	    "metadataVersion" : "1",
	    "dataVersion" : ""
    }
     */
    public class BlobEventHandler
    {
        public class Files
        {
            public Uri Header { get; set; }
            public Uri Lines { get; set; } 
            public Uri Products { get; set; }

            public bool IsComplete()
            {
                return Header != null && Lines != null && Products != null;
            }
        }

        private static ConcurrentDictionary<string, Files> batches = new ConcurrentDictionary<string, Files>();

        [FunctionName("BlobEventHandler")]
        public static async Task Run(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            [CosmosDB(
                databaseName: "Challenge2",
                collectionName: "orders",
                ConnectionStringSetting = "CosmosDBConnectionString")]
                IAsyncCollector<Order> document,
            TraceWriter log)
        {
            dynamic data = eventGridEvent.Data;

            var url = new Uri((string)data.url);

            var path = url.AbsolutePath;

            var file = path.Substring(path.LastIndexOf("/") + 1);

            var fileParts = file.Split('-');
            var batch = fileParts[0];

            // Populate Files objects
            var files = new Files();
            switch(fileParts[1])
            {
                case "OrderHeaderDetails.csv":
                    files.Header = url;
                    break;
                case "OrderLineItems.csv":
                    files.Lines = url;
                    break;
                case "ProductInformation.csv":
                    files.Products = url;
                    break;    
            }

            batches.AddOrUpdate(batch, files, (key, prev) => { 
                switch(fileParts[1])
                {
                    case "OrderHeaderDetails.csv":
                        prev.Header = url;
                        break;
                    case "OrderLineItems.csv":
                        prev.Lines = url;
                        break;
                    case "ProductInformation.csv":
                        prev.Products = url;
                        break;    
                }

                return prev;
            });
 
            var currentBatch = batches[batch];
            if(currentBatch.IsComplete())
            {
                //call to function
                log.Info($"Processing {batch} because all 3 files have been downloaded.");

                //Get Data from the Blob Storage
                string storageConnectionString = Environment.GetEnvironmentVariable("BlobConnectionString");
                CloudStorageAccount storageAccount;
                if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
                {
                    var client = storageAccount.CreateCloudBlobClient();

                    var orders = new Dictionary<string, Order>();

                    var header = await GetData(client, currentBatch.Header, log);

                    log.Info($"Parsing Header: {header.Count}");
                    foreach(var line in header)
                    {
                        orders.Add(line[0], new Order
                        {
                            ponumber = line[0],
                            datetime = line[1],
                            locationid = line[2],
                            locationname = line[3],
                            locationaddress = line[4],
                            locationpostcode = line[5],
                            totalcost = double.Parse(line[6]),
                            totaltax = double.Parse(line[7]),
                        });
                    }

                    var products = await GetData(client, currentBatch.Products, log);
                    log.Info($"Parsing Products: {products.Count}");
                    var productsDict = products.ToDictionary(p => p[0], p => new Product{
                        productid = p[0],
                        productname = p[1],
                        productdescription = p[2],
                    });

                    var lines = await GetData(client, currentBatch.Lines, log);
                    log.Info($"Parsing OrderLines: {lines.Count}");
                    foreach(var line in lines)
                    {
                        //ponumber,productid,quantity,unitcost,totalcost,totaltax
                        var orderLine = new OrderLine
                        {
                            quantity = int.Parse(line[2]),
                            unitcost = double.Parse(line[3]),
                            totalcost = double.Parse(line[4]),
                            totaltax = double.Parse(line[5]),
                            product = productsDict[line[1]]
                        };

                        orders[line[0]].orderlines.Add(orderLine);
                    }

                    foreach(var order in orders.Values)
                    {
                        log.Info($"Adding order: {order.ponumber}");
                        await document.AddAsync(order);
                    }
                }
            }
        }

        private static async Task<List<string[]>> GetData(CloudBlobClient client, Uri uri, TraceWriter log)
        {
            log.Info($"Downloading: {uri}");

            var blobRef = await client.GetBlobReferenceFromServerAsync(uri);
            
            var result = new List<string[]>(); 

            using(var stream = new MemoryStream())
            {
                await blobRef.DownloadToStreamAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using(var reader = new StreamReader(stream))
                {
                    var count = 0;
                    var line = string.Empty;
                    while((line = await reader.ReadLineAsync()) != null)
                    {
                        // ignore header
                        if (count != 0) {
                            result.Add(line.Split(','));
                        }
                        count++;
                    }
                }
            }

            return result;
        }
    }
}
