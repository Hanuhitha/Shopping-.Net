using System;
namespace Quickart_API.Models
{
    public class Home
    {
        public string? address { get; set; }
        public string? apt_suit_no { get; set; }
    }

    public class Work
    {
        public string? address { get; set; }
        public string? apt_suit_no { get; set; }
    }

    public class AddressData
    {
        public Home? home { get; set; }
        public Work? work { get; set; }
    }

    public class GetAddressesResponse
	{
        public int response_code { get; set; }
        public AddressData? data { get; set; }
        public string? response_message { get; set; }
    }
}



