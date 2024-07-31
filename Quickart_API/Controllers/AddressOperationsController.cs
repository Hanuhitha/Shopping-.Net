using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Quickart_API.Models;
using Quickart_API;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Quickart_API.Controllers
{
    [Route("AddressOperations")]
    [ApiController]
    public class AddressOperationsController : Controller
    {
        private readonly IConfiguration _configuration;

        public AddressOperationsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string validate(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Token").Value);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                string Email = (jwtToken.Claims.First(x => x.Type == "email").Value).ToString();
                string UserID = (jwtToken.Claims.First(x => x.Type == "UserID").Value).ToString();

                return UserID;
            }
            catch (Exception e)
            {
                return null;
            }

        }


        [HttpPost("FetchAddress", Name = nameof(FetchAddressAsync))]
        public async Task<ActionResult<FetchAddressResponse>> FetchAddressAsync([FromBody] FetchAddressRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new FetchAddressResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    int UserID = Convert.ToInt32(validate_token);
                    List<Address> AddressList = new List<Address>();
                    string st = ("select * from user_address where user_id=" + UserID );
                    DataTable table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);

                            myReader.Close();
                            mycon.Close();
                        }

                        if (table.Rows.Count > 0)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                Address a = new Address();
                                a.address = row["address"].ToString();
                                a.apt_suit_no = row["apt_suit_no"].ToString();
                                a.lat = row["lat"].ToString();
                                a.@long = row["long"].ToString();
                                AddressList.Add(a);
                            }
                        }
                    }

                    var response = new FetchAddressResponse
                    {
                        response_code = 200,
                        addresses = AddressList
                    };
                    return response;
                }

            }
            catch(Exception e)
            {
                var response = new FetchAddressResponse
                {
                    response_code = 404,
                    exception = e
                };

                return response;
            }

        }


        
        [HttpPost("AddAddress", Name = nameof(AddAddressAsync))]
        public async Task<ActionResult<AddAddressResponse>> AddAddressAsync([FromBody] AddAddressRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new AddAddressResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    int UserId = Convert.ToInt32(validate_token);
                    string Address = request.address;
                    string AptSuitNo = request.apt_suit_no;
                    string lat = request.lat;
                    string longitude = request.@long;
                    string AddressType = request.AddressType;

                    string st = ("insert into user_address values ("+ UserId+", '" + Address + "','" + AptSuitNo + "','" + lat + "','" + longitude + "','" + AddressType + "')");
                    DataTable table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);

                            myReader.Close();
                            mycon.Close();
                        }
                    }
                    var response = new AddAddressResponse
                    {
                        response_code = 200
                    };
                    return response;
                }
            }

            catch (Exception e)
            {
                var response = new AddAddressResponse
                {
                    exception = e,
                    response_code = 404
                };
                return response;
            }
        }


        
        [HttpPost("EditAddress", Name = nameof(EditAddressAsync))]
        public async Task<ActionResult<EditAddressResponse>> EditAddressAsync([FromBody] EditAddressRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new EditAddressResponse
                    {
                        response_code = 500,
                        response_message = "Token Auth Failed"
                    };

                    return response;
                }
                else
                {
                    int UserId = Convert.ToInt32(validate_token);
                    string Address = request.address;
                    string AptSuitNo = request.apt_suit_no;
                    string lat = request.lat;
                    string @long = request.@long;
                    string AddressType = request.AddressType;

                    string st = "UPDATE user_address SET Address ='" + Address + "', Apt_Suit_No='" + AptSuitNo + "', lat='" + lat + "', longitude='" + @long + "' WHERE User_id=" + UserId + " and Address_Type='"+ AddressType+"'";
                    DataTable table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);

                            myReader.Close();
                            mycon.Close();
                        }
                     
                    }
                    var response = new EditAddressResponse
                    {
                        response_code = 200,
                        response_message = "Address Edited"
                    };
                    return response;
                }
            }
            catch (Exception e)
            {
                var response = new EditAddressResponse
                {
                    response_code = 404,
                    response_message = e.ToString()
                };
                return response;
            }
        }



        [HttpPost("GetAddresses", Name = nameof(GetAddressesAsync))]
        public async Task<ActionResult<GetAddressesResponse>> GetAddressesAsync([FromBody] GetAddressesRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                if (validate_token == null)
                {
                    var response = new GetAddressesResponse
                    {
                        response_code = 500,
                        response_message = "Token Expired"
                    };
                    return response;
                }
                else
                {
                    int UserID = Convert.ToInt32(validate_token);
                    string st = "select address, apt_suit_no, Address_Type from user_address where user_id =" + UserID;
                    DataTable table = new DataTable();
                    string DataSource = _configuration.GetConnectionString("QuickartCon");
                    MySqlDataReader myReader;
                    Home hm = new Home();
                    Work wk = new Work();
                    AddressData d = new AddressData();
                    using (MySqlConnection mycon = new MySqlConnection(DataSource))
                    {
                        mycon.Open();
                        using (MySqlCommand mycommand = new MySqlCommand(st, mycon))
                        {
                            myReader = mycommand.ExecuteReader();
                            table.Load(myReader);

                            myReader.Close();
                            mycon.Close();
                        }

                        if (table.Rows.Count > 0)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                if (row["Address_Type"].ToString() == "Home")
                                {
                                    hm.address = row["address"].ToString();
                                    hm.apt_suit_no = row["apt_suit_no"].ToString();
                                }
                                else
                                {
                                    wk.address = row["address"].ToString();
                                    wk.apt_suit_no = row["apt_suit_no"].ToString();
                                }
                            }

                        }

                    }
                    d.home = hm;
                    d.work = wk;

                    var response = new GetAddressesResponse
                    {
                        response_code = 200,
                        data = d,
                        response_message = "Returned addresses"
                    };
                    return response;
                }

            }
            catch (Exception e)
            {
                var response = new GetAddressesResponse
                {
                    response_message = e.ToString()
                };
                return response;
            }
        }

    }
}

