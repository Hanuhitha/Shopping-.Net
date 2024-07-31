using System;
namespace Quickart_API.Models
{
	public class GetProductDetailsRequest
	{
		public string? token { get; set; }
		public string? barcode { get; set; }
		public string? storeId { get; set;  }
	}
}

