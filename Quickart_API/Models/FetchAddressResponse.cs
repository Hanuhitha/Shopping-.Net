using System;
namespace Quickart_API.Models
{


    public class Address
    {
        public string address { get; set; }
        public string apt_suit_no { get; set; }
        public string lat { get; set; }
        public string @long { get; set; }
    }

    public class FetchAddressResponse
    {
        public int response_code { get; set; }
        public List<Address> addresses { get; set; }
        public Exception exception { get; set; }
    }

}

