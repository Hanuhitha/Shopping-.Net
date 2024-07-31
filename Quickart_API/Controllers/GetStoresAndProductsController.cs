using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using Quickart_API.Models;

namespace Quickart_API.Controllers
{
    [Route("GetStoresAndProducts")]
    [ApiController]
    public class GetStoresAndProductsController : Controller
    {
        private readonly IConfiguration _configuration;

        public GetStoresAndProductsController(IConfiguration configuration)
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
                string Email = (jwtToken.Claims.First(x => x.Type== "email").Value).ToString();
                string UserID = (jwtToken.Claims.First(x => x.Type == "UserID").Value).ToString();

                return UserID;
            }
            catch (Exception e)
            {
                return null;
            }

        }


        [HttpPost("GetStores", Name = nameof(GetStoresAsync))]
        public async Task<ActionResult<GetStoresResponse>> GetStoresAsync([FromBody] GetStoresRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                string zipCode = request.zipCode;
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new GetStoresResponse
                    {
                        response_code = 500,
                        response_message = "Token authentication Failed"
                    };

                    return response;
                }
                else
                {
                    string st;
                    if (string.IsNullOrEmpty(zipCode))
                    {
                        st = ("SELECT * FROM Stores");
                    }
                    else
                    {
                        st = ("SELECT * FROM Stores where store_zipcode ='" + zipCode + "'");
                    }
                    
                    DataTable table = new DataTable();
                    List<Store> StoreList = new List<Store>();
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
                                Store s = new Store();
                                s.store_address = row["store_address"].ToString();
                                s.store_contact_number = row["store_contact_number"].ToString();
                                s.store_email = row["store_email"].ToString();
                                s.store_id = row["store_id"].ToString();
                                s.store_lat = row["store_lat"].ToString();
                                s.store_long = row["store_long"].ToString();
                                s.store_name = row["store_name"].ToString();
                                s.store_image = row["store_image"].ToString();
                                StoreList.Add(s);
                            }
                        }
                    }

                    var response = new GetStoresResponse
                    {
                        response_code = 200,
                        data = StoreList,
                        response_message = "Data Retrieved"
                    };

                    return response;
                }


            }
            catch (Exception e)
            {
                var response = new GetStoresResponse
                {
                    response_message = e.ToString()
                };
                return response;
            }
        }



        [HttpPost("GetProducts", Name = nameof(GetProductsAsync))]
        public async Task<ActionResult<GetProductsResponse>> GetProductsAsync([FromBody] GetProductsRequest request)
        {
            try
            {

                string validate_token = validate(request.token);
                bool validUser = true;
                if (validate_token == null)
                {
                    validUser = false;
                    var response = new GetProductsResponse
                    {
                        response_code = 500
                    };

                    return response;
                }
                else
                {
                    string StoreID = request.StoreID;
                    string query = request.query;
                    List<Product> ProductList = new List<Product>();
                    string st;
                    if (string.IsNullOrEmpty(query))
                    {
                        st = ("select * from Products where store_id = (select ID from stores where store_id = '" + StoreID + "')");
                        
                    }
                    else
                    {
                        st = ("select * from Products where product_name like '%"+query +"%' and store_id = (select ID from stores where store_id = '" + StoreID + "')");
                    }
                    
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
                                Product p = new Product();
                                p.product_id = row["product_id"].ToString();
                                p.product_image_url = row["product_image_url"].ToString();
                                p.product_long_description = row["product_long_description"].ToString();
                                p.product_name = row["product_name"].ToString();
                                p.product_price = Convert.ToDouble(row["product_price"].ToString());
                                p.product_short_description = row["product_short_description"].ToString();
                                p.product_barcode = row["product_barcode"].ToString();
                                p.product_weight = row["product_weight"].ToString();

                                ProductList.Add(p);
                            }
                        }
                    }

                    var response = new GetProductsResponse
                    {
                        response_code = 200,
                        data = ProductList,
                        response_message = "Data returned"
                    };

                    return response;
                }
            }
            catch (Exception e)
            {
                var response = new GetProductsResponse
                {
                    response_message = e.ToString()
                };

                return response;

            }
        }



        [HttpPost("GetProductDetails", Name = nameof(GetProductDetailsAsync))]
        public async Task<ActionResult<GetProductDetailsResponse>> GetProductDetailsAsync([FromBody] GetProductDetailsRequest request)
        {
            try
            {
                string validate_token = validate(request.token);
                if (validate_token == null)
                {
                    var response = new GetProductDetailsResponse
                    {
                        response_code = 500,
                        response_message = "Token Expired"
                    };

                    return response;
                }
                else
                {
                    var response = new GetProductDetailsResponse();
                    ProductData d = new ProductData();
                    string barcode = request.barcode;
                    if (String.IsNullOrEmpty(barcode))
                    {
                        response.response_code = 200;
                        response.response_message = "No Data";

                        return response;
                    }
                    string st = "select p.product_id, p.product_name, p.product_price, p.product_short_description, p.product_image_url, p.product_weight from quickart_db.products p, quickart_db.store_product sp, quickart_db.stores s where p.product_id = sp.product_id and p.product_barcode='" + barcode + "' and s.store_id='"+request.storeId+ "' and s.id =  p.store_id;";
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
                                d.product_id = row["product_id"].ToString();
                                d.product_image_url = row["product_image_url"].ToString();
                                d.product_name = row["product_name"].ToString();
                                d.product_price = Convert.ToInt32(row["product_price"]);
                                d.product_short_description = row["product_short_description"].ToString();
                                //d.product_qty_availability = Convert.ToInt32(row["product_qty_availability"]);
                                d.product_weight = row["product_weight"].ToString(); ;
                            }
                            response.data = d;
                        }
                        else
                            response.data = null;

                    }

                    response.response_code = 200;
                    response.response_message = "Data Returned";
                    

                    return response;
                }
            }
            catch (Exception e)
            {
                var response = new GetProductDetailsResponse
                {
                    response_message = e.ToString()
                };

                return response;
            }
        }



    }
}

