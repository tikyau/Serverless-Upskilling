using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace BFYOC
{
    public static class GetEmailTemplate
    {
        private static HttpClient client = new HttpClient { BaseAddress = new Uri("https://bfyoc-apimanagement.azure-api.net/icecream/") };

        [FunctionName("GetEmailtemplate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "emails")]HttpRequest req,
            TraceWriter log
        )
        {
            var products = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(await client.GetStringAsync("GetProducts"));

            var html = @"<!DOCTYPE html>
<html>
<body style=""background-color: whitesmoke; color: #454545; font-family:'Gill Sans', 'Gill Sans MT', Calibri, 'Trebuchet MS', sans-serif; padding-bottom: 3em;"">
    <table style=""width:100%; color:#454545"">
    <tr>
        <td style=""width:11em;"">
        <img style=""margin-left:1em;"" src=""https://serverlessohwesteurope.blob.core.windows.net/public/ice-cream-2202561_320-circle.jpg""
            height=""160"" width=""160"" alt=""Fruit Ice Cream"">
        </td>
        <td>
        <p style=""font-style: italic; font-size: 50px;  font-weight:600; margin-left: 1em;"">Best For You Organics</p>
        </td>
    </tr>
    </table>
    <p style=""text-align: center; font-style: italic; font-size: 80px;"">New Ice Cream Line!</p>
    <p style=""margin:2em 0em; font-size: 20px; text-align: center;"">Best For You Organics have a new line of fruit flavored ice creams. Below is the information so you can start the ordering process:
    </p>
    <table style=""width:100%; border-top: 1px solid #454545; border-bottom: 1px solid #454545; color:#454545; padding: 1em; font-size: 20px;"">
    <thead>
        <tr>
        <th style=""padding-bottom: 1em;"" align=""left"">Ice Cream</th>
        <th style=""padding-bottom: 1em;"" align=""left"">Description</th>
        <th style=""padding-bottom: 1em;"" align=""left"">Product ID</th>
        </tr>
    </thead>
    <tbody style=""font-size: 16px;"">
        {{loop}}
    </tbody>
    </table>
    <p style=""text-align: center; margin-top: 3em;font-size: 20px;"">Please contact your representative at Best For You Organics to get more information..</p>
</body>
</html>";
            var builder = new StringBuilder();

            foreach(var product in products)
            {
                builder.AppendLine($"<tr><td>{product.productName}</td><td>{product.productDescription}</td><td>{product.productId}</td></tr>");
            }

            html = html.Replace("{{loop}}", builder.ToString());

            return new OkObjectResult(html);
        }
    }
}
