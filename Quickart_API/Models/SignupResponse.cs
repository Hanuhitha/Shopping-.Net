using System;
namespace Quickart_API.Models
{

    public class Data
    {
        public int user_id { get; set; }
        public string token { get; set; }
    }

    public class SignupResponse
    {
        public int response_code { get; set; }
        public Data data { get; set; }
        public string response_message { get; set; }
    }

}





