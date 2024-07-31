using System;
namespace Quickart_API.Models
{
    public class GetStoresResponse
    {
        public int response_code { get; set; }
        public List<Store>? data { get; set; }
        public string? response_message { get; set; }
    }

    public class Store
    {
        public string store_id { get; set; }
        public string? store_name { get; set; }
        public string? store_address { get; set; }
        public string? store_contact_number { get; set; }
        public string? store_email { get; set; }
        public string? store_lat { get; set; }
        public string? store_long { get; set; }
        public string? store_image { get; set; }
    }
}

