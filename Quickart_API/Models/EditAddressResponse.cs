using System;
namespace Quickart_API.Models
{
	/*
	public class Address
	{
		public string address { get; set; }
		public string apt_suit_no { get; set; }
		public string lat { get; set; }
		public string @long { get; set; }
	}*/

	public class EditAddressResponse
	{
		public int response_code { get; set; }
		public string? response_message { get; set; }
		//public Address? Addr_data { get; set; }
	}
}

