using System;
namespace Quickart_API.Models
{
    public class OrderProduct
    {
        public string productId { get; set; }
        public int productQtyCnt { get; set; }
        public string productWeight { get; set; }
        public string productName { get; set; }
        public double productPrice { get; set; }
        public string productImageUrl { get; set; }
    }

    public class Datum
    {
        public string storeName { get; set; }
        public string storeImg { get; set; }
        public string storeAddress { get; set; }

        public double orderValue { get; set; }
        public int noOfProducts { get; set; }

        public string orderDate { get; set; }
        public string orderType { get; set; }
        public int orderId { get; set; }
        public string orderStatus { get; set; }
        public List<OrderProduct> orderProducts { get; set; }
    }

    public class OrderHistoryResponse
	{
		public int response_code { get; set; }
		public string response_message { get; set; }
		public List<Datum> data { get; set; }
	}
}

