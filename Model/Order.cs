using System;
using System.Collections.Generic;

namespace BFYOC
{
    public class Order
    {
        public string ponumber { get; set; }
        public string datetime { get; set; }
        public string locationid { get; set; }
        public string locationname { get; set; }
        public string locationaddress { get; set; } 
        public string locationpostcode { get; set; }
        public double totalcost { get; set; }
        public double totaltax { get; set; }
        public List<OrderLine> orderlines {get; set;} = new List<OrderLine>();
    }

    public class OrderLine
    {
        public int quantity { get; set; }
        public double unitcost { get; set; }
        public double totalcost { get; set; }
        public double totaltax { get; set; }

        public Product product { get; set; }
    }

    public class Product
    {
        public string productid { get; set; }
        public string productname { get; set; }
        public string productdescription { get; set; }
    }
}