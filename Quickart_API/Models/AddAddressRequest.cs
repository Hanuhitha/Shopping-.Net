using System;
namespace Quickart_API.Models
{
	public class AddAddressRequest
	{
        public string token { get; set; }
        //public int user_id { get; set; }
        public string? address { get; set; }
        public string? apt_suit_no { get; set; }
        public string? lat { get; set; }
        public string? @long { get; set; }
        public string? AddressType { get; set; }
    }
}

