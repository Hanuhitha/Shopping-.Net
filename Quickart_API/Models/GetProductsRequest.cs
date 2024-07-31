using System;
namespace Quickart_API.Models
{
	public class GetProductsRequest
	{
		public string StoreID { get; set; }
		public string token { get; set; }
		public string query { get; set; }
	}
}

