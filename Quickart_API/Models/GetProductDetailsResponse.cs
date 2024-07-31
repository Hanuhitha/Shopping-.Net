using System;
namespace Quickart_API.Models
{
    public class ProductData
    {
        public string product_id { get; set; }
        //public int product_qty_availability { get; set; }
        public string? product_name { get; set; }
        public int product_price { get; set; }
        public string? product_short_description { get; set; }
        public string? product_image_url { get; set; }
        public string? product_weight { get; set; }
    }

    public class GetProductDetailsResponse
	{
        public int response_code { get; set; }
        public ProductData? data { get; set; }
        public string? response_message { get; set; }
    }
}

