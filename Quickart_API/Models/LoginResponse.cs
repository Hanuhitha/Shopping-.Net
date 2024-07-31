using System;
namespace Quickart_API.Models
{

    public class DataLogin
    {
        public int user_id { get; set; }
        public string? token { get; set; }
    }

    public class LoginResponse
    {
        public int response_code { get; set; }
        public DataLogin? data { get; set; }
        public string? response_message { get; set; }
    }

}
