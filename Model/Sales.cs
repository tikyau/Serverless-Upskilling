using System;
using System.Collections.Generic;

namespace BFYOC
{
    public class Sales
    {
        public Header Header { get; set; }
        public List<Detail> Details { get; set; }
    }

    public class Detail
    {
        public string ProductId { get; set; }
        public string Quantity { get; set; }
        public string UnitCost { get; set; }
        public string TotalCost { get; set; }
        public string TotalTax { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
    }

    public class Header
    {
        public string SalesNumber { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
        public string LocationPostcode { get; set; }
        public string TotalCost { get; set; }
        public string TotalTax { get; set; }
    }

}