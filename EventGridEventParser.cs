using Microsoft.Azure.EventGrid.Models;
using System;

namespace BFYOC
{
    public class EventGridEventParser
    {
        public string Parse(EventGridEvent @event)
        {
            dynamic data = @event.Data;

            var url = new Uri((string)data.url);

            var path = url.AbsolutePath;

            var file = path.Substring(path.LastIndexOf("/") - 1);
            var batch = file.Substring(0, file.IndexOf("-"));

            return batch;
        }
    }
}
