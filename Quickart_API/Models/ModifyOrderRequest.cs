using System;
namespace Quickart_API.Models
{
	public class ModifyOrderRequest
	{
		public string token { get; set; }
		public List<int> order_ids { get; set; }
		public int user_id { get; set; }
	}
}

