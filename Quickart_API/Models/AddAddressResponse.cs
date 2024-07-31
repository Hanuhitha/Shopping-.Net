using System;
namespace Quickart_API.Models
{
	public class AddAddressResponse
	{
        public string? response_message { get; set; }
        public int response_code { get; set; }
        public Exception? exception { get; set; }
    }
}

