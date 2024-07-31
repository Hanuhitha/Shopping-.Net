using System;
namespace Quickart_API.Models
{
    public class DeliveryPerson
    {
        public int user_id { get; set; }
        public string? name { get; set; }
    }

    public class Order
    {
        public int order_id { get; set; }
        public string? store_img { get; set; }
        public string? store_name { get; set; }
        public string? store_address { get; set; }
        public string? store_address_lat { get; set; }
        public string? store_address_long { get; set; }
        public double? order_value { get; set; }
        public int? total_items { get; set; }

        public string apt_number { get; set; }
        public string delivery_address { get; set; }
        public string delivery_add_lat { get; set; }
        public string delivery_add_long { get; set; }
    }

    public class OrdersResponse
	{
		public int response_code { get; set; }
		public List<DeliveryPerson>? delivery_people { get; set; }
		public List<Order>? orders { get; set; }
        public string? response_message { get; set; }
	}
}

