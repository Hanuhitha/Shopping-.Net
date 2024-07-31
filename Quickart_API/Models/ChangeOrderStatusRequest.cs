using System;
namespace Quickart_API.Models
{
	public class ChangeOrderStatusRequest
	{
		public string token { get; set; }
		public int order_id { get; set; }
		public string order_status { get; set; }
	}
}

