using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System;

namespace BFYOC
{
    public static class Me
    {
        [FunctionName("Me")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")]HttpRequest req, TraceWriter log)
        {
            return new OkObjectResult(new { value = Environment.GetEnvironmentVariable("Region") });
        }
    }
}
