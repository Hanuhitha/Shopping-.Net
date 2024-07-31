using System;
namespace Quickart_API.Models
{
    public class ProductDetails
    {
        public string productId { get; set; }
        public int productQtyCnt { get; set; }
        public string productName { get; set; }
        public double productPrice { get; set; }
        public string productImageUrl { get; set; }
        public string productWeight { get; set; }
    }

    public class AssignedOrders
    {
        public string storeName { get; set; }
        public string storeImg { get; set; }
        public string storeAddress { get; set; }
        public string Address { get; set;}
        public double orderValue { get; set; }
        public int noOfProducts { get; set; }

        public string orderDate { get; set; }
        public string orderType { get; set; }
        public int orderId { get; set; }
        public string orderStatus { get; set; }
        public List<ProductDetails> orderProducts { get; set; }
    }

    public class AssignedOrdersResponse
	{
        public int response_code { get; set; }
        public string response_message { get; set; }
        public List<AssignedOrders> AssignedOrders { get; set; }
    }
}
